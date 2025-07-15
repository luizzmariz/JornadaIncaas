// PipeTile.cs
using UnityEngine;
using System.Collections.Generic;

public enum PipeType
{
    Straight, // Tubo reto (Horizontal ou Vertical)
    Corner,   // Tubo em L
    T_Junction, // Tubo em T (3 conexões)
    Cross,    // Tubo em Cruz (4 conexões)
    End,      // Tubo de Fim (1 conexão, como início ou fim do fluxo)
    Start     // Tubo de Início (1 conexão, como início do fluxo)
}

public class PipeTile : MonoBehaviour
{
    public PipeType pipeType;
    public SpriteRenderer spriteRenderer; // Para exibir a imagem da peça
    public Sprite[] pipeSprites; // Array de sprites para diferentes rotações/tipos
    public GameObject waterFlowEffectPrefab; // Prefab para efeito de água fluindo (opcional)

    public int gridX, gridY; // Posição na grade
    private int currentRotationIndex = 0; // 0: 0 graus, 1: 90 graus, 2: 180 graus, 3: 270 graus

    public bool isStart = false; // Indica se é o ponto de partida do fluxo
    public bool isEnd = false;   // Indica se é o ponto final do fluxo
    public bool hasWaterFlow = false; // Controla o visual do fluxo de água

    private GameManager gameManager;

    void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        // Definir sprite inicial com base no tipo e rotação
        UpdateSprite();
    }

    void OnMouseDown() // Detecta clique do mouse na peça
    {
        RotatePipe();
        if (gameManager != null)
        {
            gameManager.CheckWinCondition();
        }
    }

    public void RotatePipe()
    {
        currentRotationIndex = (currentRotationIndex + 1) % 4; // Gira 90 graus
        transform.rotation = Quaternion.Euler(0, 0, currentRotationIndex * -90); // Rotação Z no Unity é horária
        UpdateSprite();
    }

    void UpdateSprite()
    {
        // Aqui você precisará de uma lógica para escolher o sprite correto
        // com base no pipeType e na currentRotationIndex.
        // Por exemplo, você pode ter um array de sprites para cada PipeType.
        // Para simplificar, vamos apenas usar o primeiro sprite e rotacionar o GameObject.
        if (pipeSprites != null && pipeSprites.Length > 0)
        {
            spriteRenderer.sprite = pipeSprites[0]; // Ou um sprite específico para o tipo
        }
    }

    // Retorna as conexões ativas do tubo na rotação atual
    // Conexões: 0=Up, 1=Right, 2=Down, 3=Left
    public List<int> GetConnections()
    {
        List<int> connections = new List<int>();
        switch (pipeType)
        {
            case PipeType.Straight:
                // Pode ser vertical (0,2) ou horizontal (1,3) dependendo da rotação
                if (currentRotationIndex % 2 == 0) // 0 ou 2 (vertical)
                {
                    connections.Add(0); // Up
                    connections.Add(2); // Down
                }
                else // 1 ou 3 (horizontal)
                {
                    connections.Add(1); // Right
                    connections.Add(3); // Left
                }
                break;
            case PipeType.Corner:
                // Exemplo para um canto: supondo que o sprite padrão é canto inferior esquerdo (conectado a Up e Right)
                // Rotações: 0=Up+Right, 1=Right+Down, 2=Down+Left, 3=Left+Up
                if (currentRotationIndex == 0) { connections.Add(0); connections.Add(1); }
                else if (currentRotationIndex == 1) { connections.Add(1); connections.Add(2); }
                else if (currentRotationIndex == 2) { connections.Add(2); connections.Add(3); }
                else if (currentRotationIndex == 3) { connections.Add(3); connections.Add(0); }
                break;
            case PipeType.T_Junction:
                // Exemplo para um T: supondo que o sprite padrão é T sem Up (conectado a Right, Down, Left)
                // Rotações: 0=RDL, 1=ULD, 2=ULR, 3=DLR
                if (currentRotationIndex == 0) { connections.Add(1); connections.Add(2); connections.Add(3); }
                else if (currentRotationIndex == 1) { connections.Add(0); connections.Add(2); connections.Add(3); }
                else if (currentRotationIndex == 2) { connections.Add(0); connections.Add(1); connections.Add(3); }
                else if (currentRotationIndex == 3) { connections.Add(0); connections.Add(1); connections.Add(2); }
                break;
            case PipeType.Cross:
                connections.Add(0); connections.Add(1); connections.Add(2); connections.Add(3); // Todas as 4 direções
                break;
            case PipeType.End:
            case PipeType.Start:
                // Para início/fim, apenas uma conexão baseada na rotação.
                connections.Add(currentRotationIndex); // Ex: 0=Up, 1=Right, etc.
                break;
        }
        return connections;
    }

    // Determina se este tile se conecta a um vizinho específico
    public bool ConnectsTo(PipeTile neighbor, int connectionDirection)
    {
        // connectionDirection é a porta deste tile que leva ao vizinho
        // O vizinho precisa ter uma porta que se alinhe e conecte de volta
        List<int> thisConnections = GetConnections();
        if (!thisConnections.Contains(connectionDirection)) return false;

        // A porta correspondente no vizinho é a oposta
        int neighborConnectionDirection = (connectionDirection + 2) % 4; // 0->2, 1->3, 2->0, 3->1
        List<int> neighborConnections = neighbor.GetConnections();

        return neighborConnections.Contains(neighborConnectionDirection);
    }

    // Retorna a posição do vizinho para uma dada direção de conexão
    public Vector2Int GetNeighborPosition(int connectionDirection)
    {
        switch (connectionDirection)
        {
            case 0: return new Vector2Int(gridX, gridY + 1); // Up
            case 1: return new Vector2Int(gridX + 1, gridY); // Right
            case 2: return new Vector2Int(gridX, gridY - 1); // Down
            case 3: return new Vector2Int(gridX - 1, gridY); // Left
        }
        return new Vector2Int(-1, -1); // Posição inválida
    }

    public void SetIsStart(bool value)
    {
        isStart = value;
        // Adicionar feedback visual se for um tile de início
        if (isStart)
        {
            spriteRenderer.color = Color.green; // Exemplo
        }
    }

    public void SetIsEnd(bool value)
    {
        isEnd = value;
        // Adicionar feedback visual se for um tile de fim
        if (isEnd)
        {
            spriteRenderer.color = Color.red; // Exemplo
        }
    }

    public void SetWaterFlow(bool flow)
    {
        hasWaterFlow = flow;
        // Controlar a visibilidade de um efeito de água ou mudar a cor do sprite
        // Se waterFlowEffectPrefab for usado, instanciá-lo ou ativá-lo aqui.
        if (hasWaterFlow)
        {
            // Exemplo: spriteRenderer.color = Color.blue;
        }
        else
        {
            // Exemplo: spriteRenderer.color = Color.white;
        }
    }
}