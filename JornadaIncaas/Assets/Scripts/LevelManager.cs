using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public GameObject pipeTilePrefab; // Prefab da peça de tubo
    public float tileSize = 1.0f; // Tamanho de cada tile no mundo Unity
    public Transform boardTransform;

    [Header("Level Configuration")]
    public GridHolder currentPipeLayout; // Referência ao seu ScriptableObject GridHolder

    private PipeTile[,] board; // Matriz para armazenar as peças de tubo

    void Start()
    {
        if (currentPipeLayout == null)
        {
            Debug.LogError("Current Pipe Layout (GridHolder) não está definido no LevelManager. Por favor, atribua um.");
            return;
        }

        InitializeBoard();
        // GeneratePuzzle() não será mais chamado aqui, pois o layout vem do ScriptableObject
        // A lógica de "puzzle" (rotação inicial) será definida no GridHolder.
        // Podemos adicionar uma chamada para CheckWinCondition() aqui se quisermos verificar o estado inicial.
        CheckWinCondition(); // Para garantir que o estado inicial do puzzle é verificado (útil para debug)
    }

    void InitializeBoard()
    {
        int rows = currentPipeLayout.rows;
        int cols = currentPipeLayout.columns;

        board = new PipeTile[rows, cols];

        for (int x = 0; x < rows; x++) // Use rows e cols do ScriptableObject
        {
            for (int y = 0; y < cols; y++)
            {
                // Obter os dados da peça do ScriptableObject
                GridHolder.PipeData pipeData = currentPipeLayout.grid[x].values[y];

                GameObject tileGO = Instantiate(pipeTilePrefab, new Vector3(x * tileSize, y * tileSize, 0), Quaternion.identity, boardTransform);
                PipeTile pipeTile = tileGO.GetComponent<PipeTile>();

                if (pipeTile != null)
                {
                    pipeTile.gridX = x;
                    pipeTile.gridY = y;
                    board[x, y] = pipeTile;

                    // Aplicar tipo e rotação inicial da peça do ScriptableObject
                    pipeTile.pipeType = pipeData.pipeType;
                    // Rotacionar para a rotação inicial definida
                    for (int i = 0; i < pipeData.initialRotationIndex; i++)
                    {
                        pipeTile.RotatePipe(); // Chama o método de rotação para aplicar o estado inicial
                    }
                    // A chamada para UpdateSprite() dentro de RotatePipe() garantirá que o sprite correto seja definido.

                    // Você pode querer definir pontos de início/fim aqui com base nos dados do ScriptableObject,
                    // ou ter um sistema separado para isso. Por simplicidade, vou manter a atribuição manual
                    // para 0,0 e rows-1, cols-1 como exemplo, mas idealmente isso também viria do GridHolder.
                    if (pipeData.pipeType == PipeType.Start)
                    {
                        pipeTile.SetIsStart(true);
                    }
                    if (pipeData.pipeType == PipeType.End)
                    {
                        pipeTile.SetIsEnd(true);
                    }
                }
                else
                {
                    Debug.LogError("O prefab do PipeTile não possui o componente PipeTile.");
                }
            }
        }

        Camera.main.transform.position = new Vector3((rows - 1) * tileSize / 2, (cols - 1) * tileSize / 2, -10f);
        // Camera.main.orthographicSize = (Mathf.Max(rows, cols) * tileSize) / 2 + 1; // Ajuste para o maior lado
    }

    public void CheckWinCondition()
    {
        // Debug.Log("Verificando condição de vitória...");
        if (IsPathComplete())
        {
            Debug.Log("Parabéns! O puzzle foi resolvido!");
            // Adicionar lógica de vitória aqui
        }
    }

    private bool IsPathComplete()
    {
        PipeTile startTile = null;
        // Use os tamanhos do tabuleiro do GridHolder para a iteração
        int rows = currentPipeLayout.rows;
        int cols = currentPipeLayout.columns;

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                PipeTile tile = board[x,y];
                if (tile != null && tile.isStart)
                {
                    startTile = tile;
                    break;
                }
            }
            if (startTile != null) break;
        }

        if (startTile == null) return false;

        Queue<PipeTile> queue = new Queue<PipeTile>();
        HashSet<PipeTile> visited = new HashSet<PipeTile>();
        queue.Enqueue(startTile);
        visited.Add(startTile);

        bool endReached = false;

        // Reset all tiles' water flow state before checking
        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                if (board[x, y] != null)
                {
                    board[x, y].SetWaterFlow(false);
                }
            }
        }

        // Apply water flow to the starting tile
        startTile.SetWaterFlow(true);


        while (queue.Count > 0)
        {
            PipeTile current = queue.Dequeue();

            if (current.isEnd)
            {
                endReached = true;
            }

            foreach (var connection in current.GetConnections())
            {
                Vector2Int neighborPos = current.GetNeighborPosition(connection);

                if (IsValidPosition(neighborPos.x, neighborPos.y, rows, cols)) // Pass rows/cols
                {
                    PipeTile neighbor = board[neighborPos.x, neighborPos.y];

                    if (neighbor != null && !visited.Contains(neighbor) &&
                        current.ConnectsTo(neighbor, connection))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                        neighbor.SetWaterFlow(true); // Set water flow for connected tiles
                    }
                }
            }
        }
        return endReached;
    }

    // Updated IsValidPosition to take rows and cols as parameters
    private bool IsValidPosition(int x, int y, int rows, int cols)
    {
        return x >= 0 && x < rows && y >= 0 && y < cols;
    }

    public PipeTile GetTile(int x, int y)
    {
        if (currentPipeLayout == null) return null; // Safety check
        if (IsValidPosition(x, y, currentPipeLayout.rows, currentPipeLayout.columns))
        {
            return board[x, y];
        }
        return null;
    }
}