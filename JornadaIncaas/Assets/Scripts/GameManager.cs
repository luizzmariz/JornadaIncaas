// GameManager.cs
using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public GameObject pipeTilePrefab; // Prefab da peça de tubo
    public int boardSize = 5; // Tamanho do tabuleiro (e.g., 5x5)
    public float tileSize = 1.0f; // Tamanho de cada tile no mundo Unity

    private PipeTile[,] board; // Matriz para armazenar as peças de tubo

    void Start()
    {
        InitializeBoard();
        GeneratePuzzle();
    }

    void InitializeBoard()
    {
        board = new PipeTile[boardSize, boardSize];

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                GameObject tileGO = Instantiate(pipeTilePrefab, new Vector3(x * tileSize, y * tileSize, 0), Quaternion.identity);
                PipeTile pipeTile = tileGO.GetComponent<PipeTile>();
                if (pipeTile != null)
                {
                    pipeTile.gridX = x;
                    pipeTile.gridY = y;
                    board[x, y] = pipeTile;
                }
                else
                {
                    Debug.LogError("O prefab do PipeTile não possui o componente PipeTile.");
                }
            }
        }

        // Centralizar a câmera no tabuleiro (opcional)
        Camera.main.transform.position = new Vector3((boardSize - 1) * tileSize / 2, (boardSize - 1) * tileSize / 2, -10f);
        Camera.main.orthographicSize = (boardSize * tileSize) / 2 + 1;
    }

    void GeneratePuzzle()
    {
        // TODO: Implementar lógica para gerar um puzzle aleatório ou pré-definido.
        // Por enquanto, apenas para teste, vamos inicializar algumas peças.
        // Exemplo: Rotacionar algumas peças aleatoriamente no início.
        foreach (PipeTile tile in board)
        {
            if (Random.value > 0.5f) // 50% de chance de rotacionar
            {
                tile.RotatePipe();
            }
        }

        // Definir pontos de início e fim da água (exemplo)
        // Isso precisará ser mais sofisticado para puzzles complexos
        board[0, 0].SetIsStart(true); // Ponto de início
        board[boardSize - 1, boardSize - 1].SetIsEnd(true); // Ponto final
    }

    // Método a ser chamado sempre que uma peça for rotacionada
    public void CheckWinCondition()
    {
        Debug.Log("Verificando condição de vitória...");
        // TODO: Implementar a lógica para verificar se o "caminho da água" está completo.
        // Esta é a parte mais complexa e crucial do pipe puzzle.
        // Recomenda-se usar um algoritmo de busca (BFS ou DFS) a partir do ponto inicial.

        if (IsPathComplete())
        {
            Debug.Log("Parabéns! O puzzle foi resolvido!");
            // Adicionar lógica de vitória aqui (e.g., carregar próxima fase, exibir mensagem)
        }
    }

    private bool IsPathComplete()
    {
        // Algoritmo de busca para verificar o caminho da água
        // Implementação simplificada para demonstração:

        PipeTile startTile = null;
        foreach(PipeTile tile in board)
        {
            if (tile.isStart)
            {
                startTile = tile;
                break;
            }
        }

        if (startTile == null) return false;

        Queue<PipeTile> queue = new Queue<PipeTile>();
        HashSet<PipeTile> visited = new HashSet<PipeTile>();
        queue.Enqueue(startTile);
        visited.Add(startTile);

        bool endReached = false;

        while (queue.Count > 0)
        {
            PipeTile current = queue.Dequeue();

            if (current.isEnd)
            {
                endReached = true;
            }

            // Para cada porta do tile atual, verificar vizinhos
            foreach (var connection in current.GetConnections())
            {
                Vector2Int neighborPos = current.GetNeighborPosition(connection);

                if (IsValidPosition(neighborPos.x, neighborPos.y))
                {
                    PipeTile neighbor = board[neighborPos.x, neighborPos.y];

                    // Verificar se o vizinho tem uma conexão que se alinha com a porta do tile atual
                    if (neighbor != null && !visited.Contains(neighbor) &&
                        current.ConnectsTo(neighbor, connection))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }
        return endReached;
    }

    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < boardSize && y >= 0 && y < boardSize;
    }

    public PipeTile GetTile(int x, int y)
    {
        if (IsValidPosition(x, y))
        {
            return board[x, y];
        }
        return null;
    }
}