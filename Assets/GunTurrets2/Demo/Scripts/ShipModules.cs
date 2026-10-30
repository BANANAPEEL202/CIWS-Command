using UnityEngine;

/// <summary>Four ship systems share one repair crew. Timers use gameplay time.</summary>
[DisallowMultipleComponent]
public class ShipModules : MonoBehaviour
{
    public const float RepairDuration = 20f;
    public const int Engines = 2;
    [SerializeField] private Target[] modules = new Target[4];
    private readonly float[] repairedSeconds = new float[4];
    private EventLog eventLog;
    private ShipController ship;
    public int ActiveRepair { get; private set; } = -1;
    public Target Module(int index) => modules[index];
    public bool IsOperational(int index) => modules[index] != null && modules[index].isAlive;
    public float RepairProgress(int index) => Mathf.Clamp01(repairedSeconds[index] / RepairDuration);
    public float RemainingSeconds(int index) => Mathf.Max(0, RepairDuration - repairedSeconds[index]);

    private void Awake()
    {
        ship = GetComponent<ShipController>();
        eventLog = FindFirstObjectByType<EventLog>();
    }

    private void Start()
    {
        gameObject.AddComponent<ShipModuleHUD>().Initialize(this);
    }

    public bool SelectRepair(int index)
    {
        if (index < 0 || index >= 4 || IsOperational(index) || modules[index] == null ||
            (ship != null && ship.gameOverController != null && ship.gameOverController.gameOver)) return false;
        if (ActiveRepair == index) return true;
        ActiveRepair = index;
        eventLog?.AddLog("Repairing " + modules[index].targetName, new Color(1f, .82f, .4f));
        return true;
    }

    private void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i)) ||
                Input.GetKeyDown((KeyCode)((int)KeyCode.Keypad1 + i))) SelectRepair(i);
            if (IsOperational(i)) repairedSeconds[i] = 0;
        }
        if (ActiveRepair < 0) return;
        if (IsOperational(ActiveRepair)) { ActiveRepair = -1; return; }
        if (ship != null && ship.gameOverController != null && ship.gameOverController.gameOver) return;
        repairedSeconds[ActiveRepair] += Time.deltaTime;
        if (repairedSeconds[ActiveRepair] < RepairDuration) return;
        Target repaired = modules[ActiveRepair];
        repaired.ResetTarget();
        repairedSeconds[ActiveRepair] = 0;
        ActiveRepair = -1;
        eventLog?.AddLog(repaired.targetName + " repaired - online", new Color(.5f, 1f, .75f));
    }
}
