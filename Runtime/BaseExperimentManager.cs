using System;
using System.Collections.Generic;
using UnityEngine;

namespace RicKit.Experiment
{
    public interface IPrefs
    {
        void SetInt(string key, int value);
        void SetString(string key, string value);
        int GetInt(string key);
        string GetString(string key);
        void Save();
    }
    public abstract class BaseExperimentManager
    {
        protected abstract IPrefs Prefs { get; }
        private const string FirstTimeLogin = "ExperimentFirstTimeLogin";
        private const string VersionKey = "ExperimentVersion";
        private readonly Dictionary<string, BaseExperiment> experiments = new Dictionary<string, BaseExperiment>();

        /// <summary>
        /// 在注册完所有实验后调用
        /// </summary>
        public void Init()
        {
            SetGroup(Prefs.GetString(VersionKey) != Application.version, 
                Prefs.GetInt(FirstTimeLogin) == 0);
            Prefs.SetInt(FirstTimeLogin, 1);
            Prefs.SetString(VersionKey, Application.version);
            Prefs.Save();
        }

        private void SetGroup(bool firstTimeThisVersion, bool firstTimeLogin)
        {
            Debug.Log(firstTimeLogin ? "新玩家" : firstTimeThisVersion ? "新版本老玩家" : "不触发分组");
            foreach (var experiment in experiments)
            {
                var exp = experiment.Value;
                switch (exp.TargetUser)
                {
                    case ExperimentTargetUser.None:
                        break;
                    case ExperimentTargetUser.NewUser:
                        if (firstTimeLogin)
                        {
                            exp.SetGroup();
                            SaveExperiment(exp);
                        }
                        break;
                    case ExperimentTargetUser.OldUser:
                        if (!firstTimeLogin && firstTimeThisVersion)
                        {
                            exp.SetGroup();
                            SaveExperiment(exp);
                        }
                        break;
                }
                Debug.Log($"{experiment.Key}\n{exp}\n是否为实验目标：{exp.isTarget}\n分组：{exp.group}");
            }
        }
        private T LoadExperiment<T>() where T : BaseExperiment, new()
        {
            var exp = new T();
            var key = typeof(T).Name;
            var data = Prefs.GetString(key);
            return !string.IsNullOrEmpty(data) ? exp.FromString(data) as T: exp;
        }
        private void SaveExperiment<T>(T exp) where T : BaseExperiment
        {
            var key = exp.GetType().Name;
            Prefs.SetString(key, exp.ToString());
            Prefs.Save();
        }
        protected void RegisterExperiment<T>() where T : BaseExperiment, new()
        {
            var exp = LoadExperiment<T>();
            experiments[typeof(T).Name] = exp;
        }
        public void DoExperiment<T>(Action<T> action) where T : BaseExperiment
        {
            var exp = experiments[typeof(T).Name];
            action(exp as T);
        }
    }
}