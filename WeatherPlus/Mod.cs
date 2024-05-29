﻿using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Colossal.Serialization.Entities;
using Game;
using Game.Modding;
using Game.SceneFlow;
using Game.Simulation;
using Unity.Entities;
using System.Threading.Tasks;




namespace WeatherPlus
{


    public class Mod : IMod
    {
        public static ILog log = LogManager.GetLogger($"{nameof(WeatherPlus)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
        public Setting m_Setting;
        public DanielsWeatherSystem _weatherSystem;



        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));
            log.Info("Mod Loaded Successfully");
            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                log.Info($"Current mod asset at {asset.path}");


            if (_weatherSystem == null)
            {
                _weatherSystem = new DanielsWeatherSystem(this);
            }


            World.DefaultGameObjectInjectionWorld.AddSystemManaged(_weatherSystem);





            m_Setting = new Setting(this, _weatherSystem);
            m_Setting.RegisterInOptionsUI();
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(m_Setting));



            AssetDatabase.global.LoadSettings(nameof(WeatherPlus), m_Setting, new Setting(this, _weatherSystem));


            m_Setting.Apply();

            updateSystem.UpdateAt<DanielsWeatherSystem>(SystemUpdatePhase.MainLoop);
            updateSystem.UpdateAt<DanielsWeatherSystem>(SystemUpdatePhase.ApplyTool);


        }


        public void OnDispose()
        {
            log.Info("OnDispose Ran Successfully, set isInitialiszed to FALSE.");

            _weatherSystem.isInitialized = false;
        }
    }


    public partial class DanielsWeatherSystem : GameSystemBase
    {


        public ClimateSystem _climateSystem;
        public PlanetarySystem _planetarySystem;
        public bool isInitialized = false;
        public Mod _mod;
        public DanielsWeatherSystem _weather;
        public float tempTemp;
        public float tempPrecip;
        public float tempCloud;


        public DanielsWeatherSystem(Mod mod)
        {
            _mod = mod;
        }

        protected override void OnCreate()
        {
            base.OnCreate();


            Mod.log.Info("OnCreate Ran Successfully");
            _climateSystem = World.GetExistingSystemManaged<ClimateSystem>();
        }



        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);

            if (!mode.IsGameOrEditor())

                return;


            if (mode.IsGameOrEditor())
            {
                _climateSystem = World.GetExistingSystemManaged<ClimateSystem>();
                Mod.log.Info("Climate System found");

                _planetarySystem = World.GetExistingSystemManaged<PlanetarySystem>();
                Mod.log.Info("Climate System found");

                _climateSystem.temperature.overrideState = false;
                _climateSystem.precipitation.overrideState = false;
                _climateSystem.cloudiness.overrideState = false;


                if (_mod.m_Setting.EnabableTemperature == true)
                {
                    _climateSystem.temperature.overrideState = true;
                }
                else
                {
                    _climateSystem.temperature.overrideState = false;
                }
                if (_mod.m_Setting.EnablePrecipitation == true)
                {
                    _climateSystem.precipitation.overrideState = true;
                }
                else
                {
                    _climateSystem.precipitation.overrideState = false;
                }

                if (_mod.m_Setting.EnableCloudiness == true)
                {
                    _climateSystem.cloudiness.overrideState = true;
                }
                else
                {
                    _climateSystem.cloudiness.overrideState = false;
                }




                if (_mod == null)
                {
                    Mod.log.Warn("Failed to apply settings: _mod is null.");
                }
                else if (_mod.m_Setting == null)
                {
                    Mod.log.Warn("Failed to apply settings: _mod.m_Setting is null.");
                }
                else if (_climateSystem != null && _mod.m_Setting != null && isInitialized == false)
                {
                    if (_mod.m_Setting.enableTemperature == true)
                    {
                        _climateSystem.temperature.overrideValue = _mod.m_Setting.currentTemp;
                    }


                    if (_mod.m_Setting.enablePrecipitation == true)
                    {
                        _climateSystem.precipitation.overrideValue = _mod.m_Setting.currentPrecipitation;
                    }

                    if (_mod.m_Setting.enableCloudiness == true)
                    {
                        _climateSystem.cloudiness.overrideValue = _mod.m_Setting.cloudiness;
                    }



                    Mod.log.Info("Attempt Apply from GetExistingSystemManaged");
                    _mod.m_Setting.Apply();


                    isInitialized = true;

                    Mod.log.Info("Weather System Initialized");
                }
                else
                {
                    Mod.log.Info("Did not run from GetExistingSystemManaged");
                }


            }
            else
            {
                Mod.log.Info("Mode is not game or editor");
                Mod.log.Warn($"Setting is {(_climateSystem == null ? "null" : "not null")}");
            }



        }


        public void UpdateTimeOfDay(float requestedTime, bool default1, bool customTimeTrue, float customTime)
        {

            if (_planetarySystem != null && default1 == true)
            {

                _planetarySystem.overrideTime = true;
                if (customTimeTrue)
                {
                    _planetarySystem.time = customTime;
                }
                else
                {
                    _planetarySystem.time = requestedTime;
                }
            }
            else if (_planetarySystem != null && default1 == false)
            {
                _planetarySystem.overrideTime = false;
            }
            else
            {
                Mod.log.Warn("Planetary system is null, unable to update time of day.");
            }
        }


        public void UpdateWeather(float temperature, float precipitation, float cloudiness)
        {


            Task.Run(async () =>
            {
                await Task.Delay(2000);

                _climateSystem = World.GetExistingSystemManaged<ClimateSystem>();
                Mod.log.Info("UpdateWeather Ran Successfully");




                if (_climateSystem != null)
                {
                    _climateSystem.temperature.overrideState = false;
                    _climateSystem.precipitation.overrideState = false;
                    _climateSystem.cloudiness.overrideState = false;


                    if (_mod.m_Setting.EnabableTemperature == true)
                    {
                        _climateSystem.temperature.overrideState = true;
                        _climateSystem.temperature.overrideValue = temperature;
                    }
                    else
                    {
                        _climateSystem.temperature.overrideState = false;
                    }
                    if (_mod.m_Setting.EnablePrecipitation == true)
                    {
                        _climateSystem.precipitation.overrideState = true;
                        _climateSystem.precipitation.overrideValue = precipitation;
                    }
                    else
                    {
                        _climateSystem.precipitation.overrideState = false;
                    }

                    if (_mod.m_Setting.EnableCloudiness == true)
                    {
                        _climateSystem.cloudiness.overrideState = true;
                        _climateSystem.cloudiness.overrideValue = cloudiness;
                    }
                    else
                    {
                        _climateSystem.cloudiness.overrideState = false;
                    }

                    Mod.log.Info("Weather updated successfully.");

                }
                else
                {
                    Mod.log.Warn("Climate system is null, unable to update weather.");
                }
            });



        }


        protected override void OnUpdate()
        {


        }


        public void OnGameExit()
        {
            isInitialized = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }



    }

}