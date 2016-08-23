﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuSysApp;

namespace NuSysApp2
{
    public class LockDictionary : IEnumerable<KeyValuePair<string, string>>
    {
        private HashSet<string> _locals = new HashSet<string>();
        private Dictionary<string, string> _dict = new Dictionary<string, string>();
        private SessionController _workSpaceModel;
        public LockDictionary(SessionController wsm)
        {
            _workSpaceModel = wsm;
        }

        public HashSet<string> LocalLocks
        {
            get { return _locals; }
        }
        public string Value(string key)
        {
            if (_dict.ContainsKey(key))
            {
                return _dict[key];
            }
            return null;
        }
        public async Task Set(string k, string v)
        {
            if (v == "//NetworkConnector.Instance.LocalIP")//I'm so clever
            {
                _locals.Add(k);
            }
            else
            {
                _locals.Remove(k);
            }
            if (!_dict.ContainsKey(k))
            {
                _dict.Add(k, v);
            }
            else
            {
                _dict[k] = v;
            }
            await UpdateAtomLock(k, v);
        }

        private async Task UpdateAtomLock(string id, string lockHolder)
        {

            //TODO: refactor
            /*
            if (_workSpaceModel.IdToSendables.ContainsKey(id))
            {
                await UITask.Run(() => {
                    if (_workSpaceModel.IdToSendables.ContainsKey(id))
                    {
                        if (lockHolder == "")
                        {
                            _workSpaceModel.IdToSendables[id].CanEdit = ElementModel.EditStatus.Maybe;
                        }
                        else if (lockHolder == "//NetworkConnector.Instance.LocalIP")
                        {
                            var b = _workSpaceModel.IdToSendables[id];
                            b.CanEdit = ElementModel.EditStatus.Yes;
                        }
                        else
                        {
                            _workSpaceModel.IdToSendables[id].CanEdit = ElementModel.EditStatus.No;
                        }
                    }
                });
            }
            */
        }

        public void Clear()
        {
            _dict.Clear();
            _locals.Clear();
            //TODO: refactor
            /*
            foreach (KeyValuePair<string, Sendable> kvp in _workSpaceModel.IdToSendables)
            {
                kvp.Value.CanEdit = ElementModel.EditStatus.Maybe;
            }
            */
        }

        public bool ContainsID(string id)
        {
            return _dict.ContainsKey(id);
        }

        public bool ContainsHolder(string holder)
        {
            return _dict.ContainsValue(holder);
        }
        public bool RemoveID(string k)
        {
            if (_dict.ContainsKey(k))
            {
                _dict.Remove(k);
                if (_locals.Contains(k))
                {
                    _locals.Remove(k);
                }
                return true;
            }
            return false;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dict.GetEnumerator();
        }
    }
}
