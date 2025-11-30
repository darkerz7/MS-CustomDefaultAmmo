using Microsoft.Extensions.Configuration;
using Sharp.Shared;
using Sharp.Shared.GameEntities;
using Sharp.Shared.Listeners;
using Sharp.Shared.Managers;
using System.Text.Json;

namespace MS_CustomDefaultAmmo
{
    public class CustomDefaultAmmo : IModSharpModule, IGameListener, IEntityListener
    {
        public string DisplayName => "CustomDefaultAmmo";
        public string DisplayAuthor => "DarkerZ[RUS]";
        public CustomDefaultAmmo(ISharedSystem sharedSystem, string dllPath, string sharpPath, Version version, IConfiguration coreConfiguration, bool hotReload)
        {
            _modSharp = sharedSystem.GetModSharp();
            _entities = sharedSystem.GetEntityManager();
            _dllPath = dllPath;
        }

        private readonly IModSharp _modSharp;
        private readonly IEntityManager _entities;
        private readonly string _dllPath;

        ConfigWeapons? cfg = new();

        public bool Init()
        {
            _modSharp.InstallGameListener(this);
            _entities.InstallEntityListener(this);
            return true;
        }

        public void Shutdown()
        {
            _modSharp.RemoveGameListener(this);
            _entities.RemoveEntityListener(this);
        }

        public void OnEntitySpawned(IBaseEntity entity)
        {
            if (cfg != null && entity.IsValid() && entity.AsBaseWeapon() is { } weapon)
            {
                foreach (var item in cfg.Weapons.ToArray())
                {
                    if (item.DefIndex != 0 && item.DefIndex == weapon.AttributeContainer.Item.ItemDefinitionIndex)
                    {
                        if (item.Clip > -1)
                        {
                            weapon.GetWeaponData().MaxClip = item.Clip;
                            weapon.Clip = item.Clip;
                        }
                        if (item.ReserveAmmo > -1)
                        {
                            weapon.GetWeaponData().PrimaryReserveAmmoMax = item.ReserveAmmo;
                            weapon.ReserveAmmo = item.ReserveAmmo;
                        }
                        return;
                    }
                }
            }
        }

        public void OnGameActivate() //OnMapStart
        {
            LoadCFG();
        }

        public void OnGameDeactivate() //OnMapEnd
        {
            cfg = null;
        }

        void LoadCFG()
        {
            string sConfig = $"{Path.Join(_dllPath, "config.json")}";
            string sData;
            if (File.Exists(sConfig))
            {
                try
                {
                    sData = File.ReadAllText(sConfig);
                    cfg = JsonSerializer.Deserialize<ConfigWeapons>(sData);
                    if (cfg != null) //Validate
                    {
                        foreach (var weapon in cfg.Weapons.ToArray())
                        {
                            weapon.SetDefIndex();
                        }
                    }
                }
                catch
                {
                    cfg = null;
                    PrintToConsole($"Bad Config file ({sConfig})");
                }
            }
            else
            {
                cfg = null;
                PrintToConsole($"Config file ({sConfig}) not found");
            }
        }

        public static void PrintToConsole(string sValue)
        {
            Console.ForegroundColor = (ConsoleColor)8;
            Console.Write("[");
            Console.ForegroundColor = (ConsoleColor)6;
            Console.Write("CustomDefaultAmmo");
            Console.ForegroundColor = (ConsoleColor)8;
            Console.Write("] ");
            Console.ForegroundColor = (ConsoleColor)3;
            Console.WriteLine(sValue);
            Console.ResetColor();
        }

        int IGameListener.ListenerVersion => IGameListener.ApiVersion;
        int IGameListener.ListenerPriority => 0;
        int IEntityListener.ListenerVersion => IEntityListener.ApiVersion;
        int IEntityListener.ListenerPriority => 0;
    }
}
