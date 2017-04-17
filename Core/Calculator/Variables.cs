#region usings

using System;
using System.Collections.Generic;

#endregion

namespace Core.Calculator {
    public partial class InlineCalculator {
        public delegate void CalcVariableDelegate(object sender, EventArgs e);

        public const string ANSWER_VAR = "r";

        public Dictionary<string, double> Variables { get; private set; }

        public event CalcVariableDelegate OnVariableStore;

        private void LoadConstants() {
            Variables = new Dictionary<string, double> {
                {"pi", Math.PI},
                {"e", Math.E},
                {ANSWER_VAR, 0}
            };

            OnVariableStore?.Invoke(this, new EventArgs());
        }

        public void SetVariable(string name, double val) {
            if (Variables.ContainsKey(name))
                Variables[name] = val;
            else
                Variables.Add(name, val);

            OnVariableStore?.Invoke(this, new EventArgs());
        }

        public double GetVariable(string name) {
            return Variables.ContainsKey(name)
                ? Variables[name]
                : 0;
        }
    }
}