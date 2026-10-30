using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using GT2;
using GT2.Demo;

[InitializeOnLoad]
public static class CombatRegressionChecks
{
    private const string Running = "CIWS.CombatRegression";
    private static int stage, frames, errors;
    private static double deadline;
    private static Camera camera;
    private static BulletPool pool;
    private static GameObject probe, first, second, turret;
    private static Vector3 spawn = new Vector3(-280,14,75);
    private static TurretAim aim;
    private static TurretController controller;
    private static Transform firePoint;
    private static Target health;

    static CombatRegressionChecks()
    {
        EditorApplication.update += Tick;
        Application.logMessageReceived += (message, stack, type) =>
        {
            if (SessionState.GetBool(Running,false) && !stack.Contains("UnityEditor.Search.SearchDatabase") &&
                (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)) errors++;
        };
    }
    public static void Run()
    {
        Require(Application.isBatchMode,"Run this harness only in a disposable batch-mode project copy.");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SessionState.SetBool(Running,true);
        EditorApplication.EnterPlaymode();
    }
    private static void Tick()
    {
        if (!SessionState.GetBool(Running,false) || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (!Application.isPlaying || Time.frameCount < 3) return;
        try
        {
            double now = EditorApplication.timeSinceStartup;
            if(stage==0)
            {
                camera = new GameObject("Regression camera").AddComponent<Camera>();
                camera.transform.position = new Vector3(-280,40,30);
                camera.transform.LookAt(spawn);
                TestBaseOnlyAim();
                pool = CreatePool(1);
                var old = pool.GetBullet(new Vector3(250,12,0),Quaternion.identity);
                var trail = old.GetComponent<TrailRenderer>();
                trail.AddPosition(new Vector3(300,12,0));
                old.GetComponent<Rigidbody>().linearVelocity = Vector3.forward*100;
                pool.ReturnBullet(old);
                Require(!old.activeSelf && !trail.emitting && trail.positionCount==0,"Trail not cleared on return.");
                pool.ReturnBullet(old);
                probe = pool.GetBullet(spawn,Quaternion.Euler(0,35,0));
                Require(probe==old,"Expected the same pooled projectile.");
                Require(pool.GetBullet(Vector3.zero,Quaternion.identity)==null,"Duplicate return duplicated a projectile.");
                Require(Vector3.Distance(probe.GetComponent<Rigidbody>().position,spawn)<0.001f,"Physics pose not reset.");
                Require(trail.positionCount==1 && Vector3.Distance(trail.GetPosition(0),spawn)<0.001f,"Trail did not start at the muzzle.");
                probe.GetComponent<Rigidbody>().linearVelocity=Vector3.forward*10;
                stage=1; frames=0;
            }
            else if(stage==1)
            {
                camera.Render();
                if(++frames<3)return;
                var trail=probe.GetComponent<TrailRenderer>();
                for(int i=0;i<trail.positionCount;i++)
                    Require(Vector3.Distance(trail.GetPosition(i),spawn)<10,"Reused trail connected to an old/origin position.");
                pool.ReturnBullet(probe);
                UnityEngine.Object.Destroy(probe);
                UnityEngine.Object.Destroy(pool.gameObject);
                SetupTurret(); stage=2; deadline=now+5;
                Debug.Log("PASS: pooled tracer teleport, physics pose, duplicate return, and rendered trail continuity.");
            }
            else if(stage==2)
            {
                Require(now<deadline,"Aligned turret failed to fire.");
                if(Active().Count==0)return;
                Require(aim.IsAimed,"First target fired without current alignment.");
                foreach(var bullet in Active().ToArray()) pool.ReturnBullet(bullet);
                first.tag="Untagged";
                second=CreateTarget("Side target",firePoint.position+Vector3.right*90);
                aim.TraverseSpeed=0; aim.ElevationSpeed=0;
                aim.isAimed=true; // Simulate the stale flag that previously caused the burst.
                Set(controller,"fireCooldown",-2f);
                stage=3; deadline=now+0.3;
            }
            else if(stage==3)
            {
                Require(Active().Count==0,"Fired before rotating to the new target.");
                if(now<deadline)return;
                Require(controller.targetPoint==second.transform,"Did not acquire the new target.");
                Require(!aim.IsAimed,"Aimed property reused stale target state.");
                aim.TraverseSpeed=126; aim.ElevationSpeed=92;
                stage=4; deadline=now+5;
                Debug.Log("PASS: stale aimed flag and negative cooldown cannot fire at a newly acquired side target.");
            }
            else if(stage==4)
            {
                Require(now<deadline,"Turret never resumed after rotating.");
                if(Active().Count==0)return;
                Require(aim.IsAimedAt(aim.AimPosition),"Shot occurred while muzzle was unaligned.");
                Require(Active().Count<=Mathf.CeilToInt(Time.deltaTime*10)+1,"Lock delay accumulated a catch-up burst.");
                foreach(var bullet in Active())
                    Require(Vector3.Dot(bullet.GetComponent<Rigidbody>().linearVelocity.normalized,firePoint.forward)>0.999f,"Shot direction did not follow the muzzle.");
                int before=Active().Count;
                aim.IsIdle=true;
                Require(!(bool)Call(controller,"Shoot") && Active().Count==before,"Idle turret fired.");
                aim.IsIdle=false; health.isAlive=false;
                Require(!(bool)Call(controller,"Shoot"),"Destroyed turret fired.");
                health.isAlive=true; second.SetActive(false);
                Require(!(bool)Call(controller,"Shoot"),"Inactive target permitted firing.");
                Debug.Log("PASS: rotates before resuming, no stored burst, barrel-directed velocity, and idle/destroyed/inactive guards.");
                Require(errors==0,"Runtime errors during regression checks: "+errors);
                EditorSceneManager.LoadSceneInPlayMode("Assets/GunTurrets2/Demo/Game.unity",new LoadSceneParameters(LoadSceneMode.Single));
                stage=5; deadline=now+2;
            }
            else if(stage==5 && now>deadline)
            {
                var ship=UnityEngine.Object.FindAnyObjectByType<ShipController>();
                Require(ship!=null,"Gameplay scene failed to load.");
                Camera main=Camera.main;
                main.transform.position=ship.transform.position+new Vector3(40,23,46);
                main.transform.LookAt(ship.transform.position+new Vector3(0,2,6));
                var rt=new RenderTexture(1400,900,24);
                main.targetTexture=rt; main.Render(); RenderTexture.active=rt;
                var image=new Texture2D(1400,900,TextureFormat.RGB24,false);
                image.ReadPixels(new Rect(0,0,1400,900),0,0); image.Apply();
                File.WriteAllBytes(Path.Combine(Path.GetTempPath(), "ciws-muted-ocean.png"),image.EncodeToPNG());
                main.targetTexture=null; RenderTexture.active=null;
                rt.Release(); UnityEngine.Object.Destroy(rt); UnityEngine.Object.Destroy(image);
                Require(errors==0,"Gameplay preview reported errors: "+errors);
                stage=6; deadline=now+40;
            }
            else if(stage==6)
            {
                var ship=UnityEngine.Object.FindAnyObjectByType<ShipController>();
                Require(now<deadline,"Live gameplay did not intercept a missile within the test window.");
                Require(errors==0,"Live gameplay reported runtime errors: "+errors);
                if(ship == null || ship.missileCount<=1)return;
                Debug.Log("PASS: live gameplay missile interception with barrel-directed projectiles.");
                Debug.Log("ALL COMBAT REGRESSION CHECKS PASSED; preview captured.");
                SessionState.SetBool(Running,false); EditorApplication.Exit(0);
            }
        }
        catch(Exception e)
        {
            Debug.LogException(e); SessionState.SetBool(Running,false); EditorApplication.Exit(1);
        }
    }
    private static void SetupTurret()
    {
        pool=CreatePool(32);
        turret=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GunTurrets2/Demo/Prefabs/ShipTurret.prefab"));
        turret.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
        aim=turret.GetComponent<TurretAim>(); controller=turret.GetComponent<TurretController>();
        firePoint=(Transform)Get(controller,"firePoint");
        health=new GameObject("Test turret health").AddComponent<Target>(); health.enabled=false;
        Set(controller,"Target",health); Set(controller,"bulletPool",pool);
        Set(controller,"dispersion",0f); Set(controller,"fireRate",10f);
        first=CreateTarget("Forward target",firePoint.position+Vector3.forward*100);
    }
    private static BulletPool CreatePool(int count)
    {
        var host=new GameObject("Regression bullet pool"); host.SetActive(false);
        var result=host.AddComponent<BulletPool>();
        Set(result,"bulletPrefab",AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GunTurrets2/Demo/Prefabs/Bullet.prefab"));
        Set(result,"poolSize",count); Set(result,"ship",host); host.SetActive(true); return result;
    }
    private static GameObject CreateTarget(string name,Vector3 position)
    {
        var target=new GameObject(name); target.transform.position=position; target.tag="Missile";
        var body=target.AddComponent<Rigidbody>(); body.useGravity=false; body.isKinematic=true; return target;
    }
    private static void TestBaseOnlyAim()
    {
        var root=new GameObject("Base-only aim test"); root.SetActive(false); root.transform.position=new Vector3(50,0,0);
        var mount=new GameObject("Traverse").transform; mount.SetParent(root.transform,false);
        var test=root.AddComponent<TurretAim>(); Set(test,"turretBase",mount); root.SetActive(true);
        test.AimPosition=mount.position+Vector3.forward*100;
        Require(test.IsAimed,"Translated base-only turret has an incorrect angle.");
        test.isAimed=true; test.AimPosition=mount.position+Vector3.right*100;
        Require(!test.IsAimed,"Fresh angle query trusted a stale flag.");
        UnityEngine.Object.Destroy(root);
    }
    private static List<GameObject> Active(){return (List<GameObject>)Get(pool,"activeBullets");}
    private static object Get(object target,string name){return target.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(target);}
    private static void Set(object target,string name,object value){target.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(target,value);}
    private static object Call(object target,string name){return target.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,null);}
    private static void Require(bool condition,string message){if(!condition)throw new Exception(message);}
}
