using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Reflection;

using Com.A9.Singleton;
using Com.A9.FileReader;
using Com.A9.Language;

namespace Com.A9.Progression
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProgressionState
    {
        PENDING, IN_PROGRESS, COMPLETED, FINISHED
    }

    [System.Serializable]
    public class ProgressData
    {
        public string name;
        public string avatar;
        public string name_text;
        public bool auto_finish;
        public string desc;
        public int sprite_index;

        public string entry_point;
        public string complete_condition;
        public string complete_function;
        public string reward_function;

        public string GetDesc()
        {
            return desc.Split(',')[(int)CommonLanguage.language];
        }
        public string GetName()
        {
            return name_text.Split(',')[(int)CommonLanguage.language];
        }
    }

    [System.Serializable]
    public class ProgressRecord
    {
        public string name;
        public ProgressionState status;
    }

    public class Progression : Singleton<Progression>
    {
        public Dictionary<string, ProgressData> progression = new Dictionary<string, ProgressData>();
        public List<ProgressRecord> records = new List<ProgressRecord>();
        public delegate void EveHandler(string ms);

        public event Action CheckAllProgressions;
        public event EveHandler OnComplete;
        public event EveHandler OnFinish;
        public event EveHandler OnEnter;

        void Start()
        {
            Init();
        }

        public void Init()
        {
            progression = new Dictionary<string, ProgressData>();
            List<ProgressData> pgs = new List<ProgressData>();
            pgs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProgressData>>(Resources.Load<TextAsset>("GameData/progression").text);
            for (int i = 0; i < pgs.Count; i++)
            {
                progression.Add(pgs[i].name, pgs[i]);
            }

            foreach (var item in progression)
            {
                RegisterEntry(item.Value.name, item.Value.entry_point);
            }
            foreach (var item in progression)
            {
                RegisterEvent(item.Value.name, item.Value.complete_condition);
            }

            xmlReader.ReadJson<List<ProgressRecord>>("progression_records.json", out records);
            if (records == null || records.Count == 0)
            {
                records = new List<ProgressRecord>();
                foreach (var item in progression)
                {
                    records.Add(new ProgressRecord() { name = item.Value.name, status = ProgressionState.PENDING });
                }
            }
            Save();
            CheckAll();
        }

        public void Save()
        {
            xmlReader.SaveAsJson<List<ProgressRecord>>("progression_records.json", records);
        }

        public void CheckAll()
        {
            CheckAllProgressions?.Invoke();
        }

        public void UnlockAll()
        {
            foreach (var item in progression)
            {
                Debug.Log(item.Key);
                StartQuest(item.Key);
                Complete(item.Key);
                Finish(item.Key);
            }
        }

        public void RegisterEvent(string quest, string func)
        {
            if (string.IsNullOrEmpty(func))
            {
                CheckAllProgressions += () =>
                {
                    Complete(quest);
                };
                return;
            }
            string[] parse = func.Split(' ');
            string nm = parse[0];

            var lst = parse.ToList();
            lst.RemoveAt(0);
            object[] args = lst.ToArray();

            MethodInfo md = typeof(ProgressionEvents).GetMethod(nm, BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);

            CheckAllProgressions += () =>
            {
                bool ret = (bool)md.Invoke(null, args);
                if (ret) Complete(quest);
            };
        }

        public void RegisterEntry(string quest, string func)
        {
            if (string.IsNullOrEmpty(func) == false)
            {
                string[] parse = func.Split(' ');
                string nm = parse[0];

                var lst = parse.ToList();
                lst.RemoveAt(0);
                object[] args = lst.ToArray();

                MethodInfo md = typeof(ProgressionEvents).GetMethod(nm, BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
                CheckAllProgressions += () =>
                {
                    bool ret = (bool)md.Invoke(null, args);
                    if (ret) StartQuest(quest);
                };
            }
            else
            {
                CheckAllProgressions += () =>
                {
                    StartQuest(quest);
                };
            }
        }

        public void Reward(string func)
        {
            if (string.IsNullOrEmpty(func))
            {
                return;
            }

            string[] all = func.Split(',');
            for (int i = 0; i < all.Length; i++)
            {
                string[] parse = all[i].Split(' ');
                string nm = parse[0];

                var lst = parse.ToList();
                lst.RemoveAt(0);
                object[] args = lst.ToArray();

                MethodInfo md = typeof(ProgressionRewards).GetMethod(nm, BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
                md.Invoke(null, args);
            }
        }

        public bool IsComplete(string nm)
        {
            if (records.Find(c => c.name == nm).status == ProgressionState.COMPLETED)
                return true;
            return false;
        }

        public bool IsFinished(string nm)
        {
            if (records.Find(c => c.name == nm).status == ProgressionState.FINISHED)
                return true;
            return false;
        }

        public void Complete(string st)
        {
            if (records.Find(c => c.name == st).status != ProgressionState.IN_PROGRESS) return;
            records.Find(c => c.name == st).status = ProgressionState.COMPLETED;
            Reward(progression[st].complete_function);
            OnComplete?.Invoke(st);
            Save();
            if (progression[st].auto_finish)
            {
                Finish(st);
            }
        }

        public void Finish(string st)
        {
            if (records.Find(c => c.name == st).status != ProgressionState.COMPLETED) return;
            records.Find(c => c.name == st).status = ProgressionState.FINISHED;
            // SteamIntergation.Unlock(st);
            Reward(progression[st].reward_function);
            OnFinish?.Invoke(st);
            Save();
            CheckAll();
        }

        public void StartQuest(string st)
        {
            if (records.Find(c => c.name == st).status != ProgressionState.PENDING) return;
            records.Find(c => c.name == st).status = ProgressionState.IN_PROGRESS;
            OnEnter?.Invoke(st);
            Save();
        }
    }
}


