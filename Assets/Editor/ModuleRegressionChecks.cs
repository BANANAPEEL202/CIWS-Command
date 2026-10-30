using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class ModuleRegressionChecks
{
    private const string Running = "CIWS.ModuleRegression";
    private static ShipController ship;
    private static ShipModules modules;
    private static Radar radar;
    private static int stage, errors;
    private static float deadline, paused, started, coastFrom;
    private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

    static ModuleRegressionChecks()
    {
        EditorApplication.update += Tick;
        Application.logMessageReceived += (message, stack, type) =>
        {
            if (SessionState.GetBool(Running, false) && !stack.Contains("UnityEditor.Search.SearchDatabase") &&
                (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)) errors++;
        };
    }
    public static void Run()
    {
        Check(Application.isBatchMode, "Use a disposable batch-mode project.");
        GraphicsValidation.Validate();
        EditorSceneManager.OpenScene("Assets/GunTurrets2/Demo/Game.unity");
        SessionState.SetBool(Running, true);
        EditorApplication.EnterPlaymode();
    }
    private static void Tick()
    {
        if (!SessionState.GetBool(Running, false) || EditorApplication.isCompiling || EditorApplication.isUpdating ||
            !Application.isPlaying || Time.frameCount < 4) return;
        try
        {
            if (stage == 0)
            {
                ship = UnityEngine.Object.FindAnyObjectByType<ShipController>();
                modules = ship.GetComponent<ShipModules>();
                radar = UnityEngine.Object.FindAnyObjectByType<Radar>();
                Check(modules != null, "Modules missing from gameplay ship.");
                Check(Enumerable.Range(0, 4).Select(modules.Module).Distinct().Count() == 4, "Module targets must be unique.");
                Check(Enumerable.Range(0, 4).All(modules.IsOperational), "A module starts offline.");
                Check(!modules.SelectRepair(0) && !modules.SelectRepair(-1) && !modules.SelectRepair(4), "Healthy/invalid repair accepted.");
                var turrets = ship.GetComponentsInChildren<GT2.Demo.TurretController>();
                Check(turrets.Length == 2, "Expected exactly two turret modules.");
                var linked = turrets.Select(t => (Target)t.GetType().GetField("Target", Private).GetValue(t)).ToArray();
                Check(linked.Contains(modules.Module(0)) && linked.Contains(modules.Module(1)), "Turret health wiring differs from repair modules.");
                var spawner = UnityEngine.Object.FindAnyObjectByType<MissileSpawner>();
                spawner.StopAllCoroutines(); spawner.enabled = false;
                var missile = UnityEngine.Object.Instantiate(spawner.MissilePrefab,
                    modules.Module(2).transform.position, Quaternion.identity).GetComponent<Missile>();
                missile.target = modules.Module(2).transform;
                missile.enabled = false;
                missile.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                ship.currentSpeedState = ShipController.SpeedState.FullForward;
                ship.currentForwardSpeed = 8;
                ship.GetComponent<Rigidbody>().linearVelocity = Vector3.forward * 8;
                Physics.SyncTransforms();
                deadline = Time.time + 3; stage = 1;
            }
            else if (stage == 1)
            {
                Check(Time.time < deadline, "Actual missile overlap failed to damage engines.");
                if (modules.IsOperational(2)) return;
                Check(!ship.EnginesOperational, "Engine module not connected to ship.");
                coastFrom = ship.currentForwardSpeed;
                stage = 2; deadline = Time.time + .2f;
            }
            else if (stage == 2 && Time.time > deadline)
            {
                Check(ship.currentSpeedState == ShipController.SpeedState.Stop, "Engine failure did not set the throttle to Stop.");
                Check(ship.currentForwardSpeed > 0 && ship.currentForwardSpeed < coastFrom, "Engine failure should coast down, not stop instantly.");
                Check(ship.GetComponent<Rigidbody>().linearVelocity.sqrMagnitude > .01f, "Ship stopped dead instead of coasting.");
                Debug.Log("PASS: real missile impact disables engines and the ship coasts down as if stopped.");
                var blip = UnityEngine.Object.Instantiate(radar.radarBlipPrefab, radar.radarBlips);
                radar.StartCoroutine((System.Collections.IEnumerator)radar.GetType().GetMethod("FadeOutBlip", Private).Invoke(radar, new object[] { blip }));
                // Three modules down (engines, forward turret, radar) must stay recoverable.
                modules.Module(0).KillTarget(); modules.Module(3).KillTarget();
                Check(modules.SelectRepair(0), "Cannot select damaged turret.");
                Time.timeScale = 5; started = Time.time; stage = 3;
            }
            else if (stage == 3)
            {
                if (Time.time - started < .2f) return;
                Check(!modules.IsOperational(0), "Turret repaired before 20 seconds.");
                Check(Mathf.Abs(Mathf.DeltaAngle(0, ship.transform.localEulerAngles.z)) <= ship.rockingStrength + .1f,
                    "Engine-off wave roll accumulates beyond its configured amplitude.");
                Check(!ship.gameOverController.gameOver, "Three modules down ended the game early.");
                Check(!radar.radarBeam.gameObject.activeSelf && !radar.radarBlips.gameObject.activeSelf &&
                    !radar.cameraPOV.gameObject.activeSelf && !radar.radarAudioSource.isPlaying, "Radar remains active offline.");
                Check(radar.radarBlips.childCount == 0, "Offline radar retains stale contacts.");
                foreach (var turret in ship.GetComponentsInChildren<GT2.Demo.TurretController>())
                    if ((Target)turret.GetType().GetField("Target", Private).GetValue(turret) == modules.Module(0))
                        Check(!(bool)turret.GetType().GetMethod("Shoot", Private).Invoke(turret, null), "Offline turret fired.");
                if (Time.time - started < 4) return;
                paused = modules.RepairProgress(0);
                modules.SelectRepair(2); started = Time.time; stage = 4;
            }
            else if (stage == 4)
            {
                Check(Mathf.Approximately(modules.RepairProgress(0), paused), "Two modules repaired at once.");
                if (Time.time - started < 3) return;
                float progress = modules.RepairProgress(2);
                Check(progress > 0 && progress < .25f, "Engine repair progress incorrect.");
                modules.SelectRepair(2);
                Check(modules.RepairProgress(2) == progress, "Repeated key resets repair timer.");
                modules.SelectRepair(0); started = Time.time; stage = 5;
            }
            else if (stage == 5)
            {
                if (!modules.IsOperational(0))
                {
                    Check(Time.time - started < 21, "Repair did not complete.");
                    return;
                }
                Check(Time.time - started >= 20 * (1 - paused) - .5f, "Repair completed early.");
                Check(!modules.IsOperational(2) && !modules.IsOperational(3), "Repair healed other modules.");
                Check(modules.ActiveRepair == -1, "Crew not released after repair.");
                Check(!modules.Module(0).fireEffect.activeSelf, "Repaired module is still burning.");
                Debug.Log("PASS: one repair crew, paused progress, full 20-second repair, offline systems and three-down recovery.");
                modules.SelectRepair(2); started = Time.time; stage = 6;
            }
            else if (stage == 6)
            {
                Check(Time.time - started < 21, "Engine repair failed.");
                if (!modules.IsOperational(2)) return;
                Check(ship.EnginesOperational, "Repaired engines remain locked.");
                ship.currentSpeedState = ShipController.SpeedState.FullForward;
                stage = 7; deadline = Time.time + 1;
            }
            else if (stage == 7 && Time.time > deadline)
            {
                Check(ship.currentForwardSpeed > 0 && ship.GetComponent<Rigidbody>().linearVelocity.sqrMagnitude > .01f,
                    "Repaired engines cannot propel the ship.");
                modules.SelectRepair(3); started = Time.time; stage = 8;
            }
            else if (stage == 8)
            {
                Check(Time.time - started < 21, "Radar repair failed.");
                if (!modules.IsOperational(3)) return;
                stage = 9; deadline = Time.time + .3f;
            }
            else if (stage == 9 && Time.time > deadline)
            {
                Check(radar.radarBeam.gameObject.activeSelf && radar.radarBlips.gameObject.activeSelf && radar.cameraPOV.gameObject.activeSelf,
                    "Radar did not resume after repair.");
                modules.Module(0).KillTarget();
                Check(modules.RepairProgress(0) == 0 && modules.SelectRepair(0), "Repeated damage/repair cycle failed.");
                Time.timeScale = 0; started = modules.RemainingSeconds(0); stage = 10;
                deadline = (float)EditorApplication.timeSinceStartup + .4f;
            }
            else if (stage == 10 && EditorApplication.timeSinceStartup > deadline)
            {
                Check(Mathf.Approximately(modules.RemainingSeconds(0), started), "Repair advances while gameplay is paused.");
                Time.timeScale = 1;
                Capture();
                Check(errors == 0, "Runtime errors: " + errors);
                stage = 11; deadline = (float)EditorApplication.timeSinceStartup + 1;
            }
            else if (stage == 11 && EditorApplication.timeSinceStartup > deadline)
            {
                Check(File.Exists("/tmp/ciws-damage-control.png"), "HUD screenshot was not saved.");
                // Losing the other three while the forward turret is still under repair ends the game.
                Check(modules.ActiveRepair == 0, "Forward turret should still be under repair.");
                for (int i = 1; i < 4; i++) modules.Module(i).KillTarget();
                stage = 12; deadline = (float)EditorApplication.timeSinceStartup + .3f;
            }
            else if (stage == 12 && EditorApplication.timeSinceStartup > deadline)
            {
                Check(ship.gameOverController.gameOver, "All four modules down (one mid-repair) did not end the game.");
                Check(!modules.SelectRepair(1), "Repairs accepted after game over.");
                Check(GameObject.Find("Damage Control HUD") == null, "Repair keys still shown after game over.");
                Debug.Log("ALL MODULE REGRESSION CHECKS PASSED: actual missile impact, engine coast-down/recovery, radar shutdown/recovery, offline turret guard, three-down recovery, all-four-down game over with hidden repair keys, 20-second repairs, switching, single crew, repeated selection/damage and pause.");
                SessionState.SetBool(Running, false); EditorApplication.Exit(0);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e); SessionState.SetBool(Running, false); EditorApplication.Exit(1);
        }
    }
    private static void Capture()
    {
        Camera camera = Camera.main;
        camera.transform.position = ship.transform.position + new Vector3(28, 16, 31);
        camera.transform.LookAt(ship.transform.position + new Vector3(0, 2, 6));
        var overlays = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)
            .Where(c => c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
        var rt = new RenderTexture(1400, 900, 24);
        var previous = RenderTexture.active;
        camera.targetTexture = rt;
        foreach (Canvas canvas in overlays)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera; canvas.planeDistance = 1;
        }
        Canvas.ForceUpdateCanvases();
        camera.Render(); RenderTexture.active = rt;
        var texture = new Texture2D(1400, 900, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, 1400, 900), 0, 0); texture.Apply();
        File.WriteAllBytes("/tmp/ciws-damage-control.png", texture.EncodeToPNG());
        foreach (Canvas canvas in overlays) canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        camera.targetTexture = null; RenderTexture.active = previous;
        rt.Release(); UnityEngine.Object.Destroy(rt); UnityEngine.Object.Destroy(texture);
    }
    private static void Check(bool value, string message) { if (!value) throw new Exception(message); }
}
