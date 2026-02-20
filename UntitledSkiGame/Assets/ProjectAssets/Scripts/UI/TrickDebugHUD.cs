using System.Text;
using Global_Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class TrickDebugHUD : MonoBehaviour
{
    [Header("Throw SkiMovement here xd")]
    public SkiMovement skiMovement;

    [Header("HUD")]
    public bool showHud = true;
    public int fontSize = 22;
    private string cachedKeys = "";
    private string cachedActions = "";
    public float refreshRateHz = 10f;
    private float nextRefreshTime = 0f;

    private GlobalInput input;

    private GUIStyle centeredStyle;
    private GUIStyle centeredStyleGreen;
    private GUIStyle centeredStyleRed;

    private void OnEnable()
    {
        input = new GlobalInput();

        input.Mounted.Enable();
        input.Player.Enable();
        input.Unmounted.Enable();

        centeredStyle = new GUIStyle
        {
            fontSize = fontSize,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };

        centeredStyleGreen = new GUIStyle(centeredStyle);
        centeredStyleGreen.normal.textColor = Color.green;

        centeredStyleRed = new GUIStyle(centeredStyle);
        centeredStyleRed.normal.textColor = Color.red;
    }

    private void OnDisable()
    {
        if (input == null) return;
        input.Mounted.Disable();
        input.Player.Disable();
        input.Unmounted.Disable();
    }

    private void Update()
    {
        // Toggle HUD on/off with F1 
        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
        {
            showHud = !showHud;
        }
    }

    private void OnGUI()
    {
        if (!showHud) return;

        float centerX = Screen.width * 0.5f;
        float centerY = Screen.height * 0.15f;
        float width = 1200f;
        float lineH = fontSize + 10;

        bool grounded = skiMovement != null && skiMovement.grounded;

        // Refresh text only at fixed intervals 
        if (Time.unscaledTime >= nextRefreshTime)
        {
            nextRefreshTime = Time.unscaledTime + (1f / Mathf.Max(1f, refreshRateHz));

            cachedKeys = BuildKeyboardLine();
            cachedActions = BuildActionsLine();
        }

        // 1) State line (colored)
        string stateText = grounded ? "GROUNDED" : "IN AIR";
        GUIStyle stateStyle = grounded ? centeredStyleGreen : centeredStyleRed;
        GUI.Label(new Rect(centerX - width / 2f, centerY, width, lineH), stateText, stateStyle);

        // 2) Keys line
        GUI.Label(new Rect(centerX - width / 2f, centerY + lineH, width, lineH), cachedKeys, centeredStyle);

        // 3) Actions line
        GUI.Label(new Rect(centerX - width / 2f, centerY + 2 * lineH, width, lineH), cachedActions, centeredStyle);
    }

    private string BuildKeyboardLine()
    {
        // Use the new Input System Keyboard
        if (Keyboard.current == null) return "KEYS: (no keyboard detected)";

        var sb = new StringBuilder("KEYS: ");
        bool any = false;

        AppendKey(sb, Keyboard.current.wKey, "W", ref any);
        AppendKey(sb, Keyboard.current.aKey, "A", ref any);
        AppendKey(sb, Keyboard.current.sKey, "S", ref any);
        AppendKey(sb, Keyboard.current.dKey, "D", ref any);

        AppendKey(sb, Keyboard.current.spaceKey, "Space", ref any);
        AppendKey(sb, Keyboard.current.leftShiftKey, "LShift", ref any);
        AppendKey(sb, Keyboard.current.leftCtrlKey, "LCtrl", ref any);
        AppendKey(sb, Keyboard.current.eKey, "E", ref any);
        AppendKey(sb, Keyboard.current.qKey, "Q", ref any);

        if (!any) sb.Append("(none)");
        return sb.ToString();
    }

    private void AppendKey(StringBuilder sb, KeyControl key, string label, ref bool any)
    {
        if (key != null && key.isPressed)
        {
            sb.Append(label).Append("  ");
            any = true;
        }
    }

    private string BuildActionsLine()
    {
        if (input == null) return "ACTIONS: (input not ready)";

        var sb = new StringBuilder("ACTIONS: ");
        bool any = false;

        // Print all actions from Mounted, Player, Unmounted that are non-default.
        AppendMapActions(sb, input.asset.FindActionMap("Mounted", false), "Mounted", ref any);
        AppendMapActions(sb, input.asset.FindActionMap("Player", false), "Player", ref any);
        AppendMapActions(sb, input.asset.FindActionMap("Unmounted", false), "Unmounted", ref any);

        if (!any) sb.Append("(none)");
        return sb.ToString();
    }

    private void AppendMapActions(StringBuilder sb, InputActionMap map, string mapName, ref bool any)
    {
        if (map == null) return;

        foreach (var action in map.actions)
        {
            if (!action.enabled) continue;

            // Filter: Look is continuous mouse delta and will spam constantly
            if (mapName == "Player" && action.name == "Look")
                continue;

            // Button actions
            if (action.type == InputActionType.Button)
            {
                if (action.IsPressed())
                {
                    sb.Append($"{mapName}.{action.name}  ");
                    any = true;
                }
                continue;
            }

            // Value actions: print when not default
            object v = action.ReadValueAsObject();
            if (v == null) continue;

            if (v is float f)
            {
                if (Mathf.Abs(f) > 0.01f)
                {
                    sb.Append($"{mapName}.{action.name}={f:0.00}  ");
                    any = true;
                }
            }
            else if (v is Vector2 v2)
            {
                if (v2.sqrMagnitude > 0.001f)
                {
                    sb.Append($"{mapName}.{action.name}={v2}  ");
                    any = true;
                }
            }
            else if (v is Vector3 v3)
            {
                if (v3.sqrMagnitude > 0.001f)
                {
                    sb.Append($"{mapName}.{action.name}={v3}  ");
                    any = true;
                }
            }
            else
            {
                sb.Append($"{mapName}.{action.name}={v}  ");
                any = true;
            }
        }
    }
}