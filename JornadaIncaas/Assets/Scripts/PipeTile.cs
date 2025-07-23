// PipeTile.cs
using UnityEngine;
using System.Collections.Generic;

public enum PipeType
{
    Straight, // Tubo reto (Horizontal ou Vertical)
    Corner,   // Tubo em L
    T_Junction, // Tubo em T (3 conexões)
    Cross,    // Tubo em Cruz (4 conexões)
    End,      // Tubo de Fim (1 conexão, como inicio ou fim do fluxo)
    Start     // Tubo de Inicio (1 conexão, como início do fluxo)
}

public class PipeTile : MonoBehaviour
{
    // Renderizador principal do sprite do tubo
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer tileSpriteRenderer;

    // NOVO: Renderizadores para as conexões (portas)
    public SpriteRenderer connectionUpRenderer;
    public SpriteRenderer connectionRightRenderer;
    public SpriteRenderer connectionDownRenderer;
    public SpriteRenderer connectionLeftRenderer;

    // Sprites para os diferentes tipos de tubos (mantidos como antes)
    public Sprite straightPipeSprite;
    public Sprite cornerPipeSprite;
    public Sprite tJunctionSprite;
    public Sprite crossPipeSprite;
    public Sprite endPipeSprite;
    public Sprite startPipeSprite;

    public GameObject waterFlowEffectPrefab; // Prefab para efeito de água fluindo (opcional)

    public int gridX, gridY; // Posição na grade
    [SerializeField] private int currentRotationIndex = 0; // 0: 0 graus, 1: 90 graus, 2: 180 graus, 3: 270 graus

    public PipeType pipeType; // Definido pelo LevelManager

    public bool isStart = false; // Indica se é o ponto de partida do fluxo
    public bool isEnd = false;   // Indica se é o ponto final do fluxo
    public bool hasWaterFlow = false; // Controla o visual do fluxo de água para o tubo inteiro

    private LevelManager levelManager;
    private Color32 originalColor; // Para armazenar a cor original do sprite principal
    public Color32 highlightColor = new Color32(204, 230, 255, 255); // Cor do brilho ao passar o mouse
    public Color32 waterFlowColor = new Color32(0, 100, 255, 255); // Cor para o fluxo de água (azul vibrante)
    public Color32 connectionDefaultColor; // Cor padrão das conexões

    bool pipeIsActive = false;


    void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    void Start()
    {
        levelManager = FindFirstObjectByType<LevelManager>();
        UpdateSprite(); // Definir sprite inicial com base no tipo e rotação
    }

    void OnMouseEnter() // Detecta se o mouse entrou no espaço da peça
    {
        if (pipeIsActive)
        {
            tileSpriteRenderer.color = highlightColor;
        }
    }

    void OnMouseExit() // Detecta se o mouse saiu do espaço da peça
    {
        if (pipeIsActive)
        {
        tileSpriteRenderer.color = originalColor;    
        }
    }

    void OnMouseDown() // Detecta clique do mouse na peça
    {
        if (pipeIsActive)
        {
            RotatePipe();
            if (levelManager != null)
            {
                levelManager.CheckWinCondition();
            }    
        }
    }

    public void RotatePipe()
    {
        currentRotationIndex = (currentRotationIndex + 1) % 4; // Gira 90 graus 
        transform.rotation = Quaternion.Euler(0, 0, currentRotationIndex * -90); // Rotação Z no Unity é horaria 
        UpdateSprite();
        ResetConnectionColors(); // NOVO: Redefine as cores das conexões ao rotacionar
    }

    void UpdateSprite()
    {
        switch (pipeType)
        {
            case PipeType.Straight:
                spriteRenderer.sprite = straightPipeSprite;
                break;
            case PipeType.Corner:
                spriteRenderer.sprite = cornerPipeSprite;
                break;
            case PipeType.T_Junction:
                spriteRenderer.sprite = tJunctionSprite;
                break;
            case PipeType.Cross:
                spriteRenderer.sprite = crossPipeSprite;
                break;
            case PipeType.End:
                spriteRenderer.sprite = endPipeSprite;
                break;
            case PipeType.Start:
                spriteRenderer.sprite = startPipeSprite;
                break;
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
                if (currentRotationIndex % 2 == 0)
                {
                    connections.Add(1); // Right 
                    connections.Add(3); // Left 
                }
                else
                {
                    connections.Add(0); // Up 
                    connections.Add(2); // Down 
                }
                break;
            case PipeType.Corner:
                if (currentRotationIndex == 0) { connections.Add(0); connections.Add(1); }
                else if (currentRotationIndex == 1) { connections.Add(1); connections.Add(2); }
                else if (currentRotationIndex == 2) { connections.Add(2); connections.Add(3); }
                else if (currentRotationIndex == 3) { connections.Add(3); connections.Add(0); }
                break;
            case PipeType.T_Junction:
                if (currentRotationIndex == 0) { connections.Add(0); connections.Add(1); connections.Add(3); }
                else if (currentRotationIndex == 1) { connections.Add(0); connections.Add(1); connections.Add(2); }
                else if (currentRotationIndex == 2) { connections.Add(1); connections.Add(2); connections.Add(3); }
                else if (currentRotationIndex == 3) { connections.Add(2); connections.Add(3); connections.Add(0); }
                break;
            case PipeType.Cross:
                connections.Add(0); connections.Add(1); connections.Add(2); connections.Add(3); // Todas as 4 direções
                break;
            case PipeType.End:
            case PipeType.Start:
                connections.Add(currentRotationIndex); // Ex: 0=Up, 1=Right, etc. 
                break;
        }
        return connections;
    }

    // Determina se este tile se conecta a um vizinho específico 
    public bool ConnectsTo(PipeTile neighbor, int connectionDirection)
    {
        List<int> thisConnections = GetConnections();
        if (!thisConnections.Contains(connectionDirection)) return false;

        int neighborConnectionDirection = (connectionDirection + 2) % 4;
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
        if (isStart)
        {
            spriteRenderer.color = Color.green;
        }
    }

    public void SetIsEnd(bool value)
    {
        isEnd = value;
        if (isEnd)
        {
            spriteRenderer.color = Color.red;
        }
    }

    // NOVO: Método para redefinir a cor de todos os renderers de conexão para a cor padrão
    private void ResetConnectionColors()
    {
        if (connectionUpRenderer != null) connectionUpRenderer.color = connectionDefaultColor;
        if (connectionRightRenderer != null) connectionRightRenderer.color = connectionDefaultColor;
        if (connectionDownRenderer != null) connectionDownRenderer.color = connectionDefaultColor;
        if (connectionLeftRenderer != null) connectionLeftRenderer.color = connectionDefaultColor;
    }

    public void SetWaterFlow(bool flow)
    {
        hasWaterFlow = flow;

        // NOVO: Lógica para colorir os sprites das conexões
        ResetConnectionColors(); // Começa redefinindo todas as cores das conexões

        if (hasWaterFlow)
        {
            List<int> activeConnections = GetConnections(); // Obtém as conexões ativas do tipo de tubo e rotação

            switch (pipeType)
            {
                case PipeType.Straight:
                    connectionRightRenderer.color = waterFlowColor;
                    connectionLeftRenderer.color = waterFlowColor;
                    break;
                case PipeType.Corner:
                    connectionUpRenderer.color = waterFlowColor;
                    connectionRightRenderer.color = waterFlowColor;
                    break;
                case PipeType.T_Junction:
                    connectionUpRenderer.color = waterFlowColor;
                    connectionRightRenderer.color = waterFlowColor;
                    connectionLeftRenderer.color = waterFlowColor;
                    break;
                case PipeType.Cross:
                    connectionUpRenderer.color = waterFlowColor;
                    connectionRightRenderer.color = waterFlowColor;
                    connectionDownRenderer.color = waterFlowColor;
                    connectionLeftRenderer.color = waterFlowColor;
                    break;
                case PipeType.End:
                case PipeType.Start:
                    connectionUpRenderer.color = waterFlowColor;
                    break;
            }
        }
    }

    public void ActivatePipe()
    {
        pipeIsActive = true;
    }
}