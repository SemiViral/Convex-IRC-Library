#region usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

#endregion

namespace Convex.Plugin.Calculator {
    public partial class InlineCalculator {
        private string expression;
        
        private Stack<double> operands;
        private Stack<string> operators;

        private string token;
        private int tokenPos;

        public void Clear() {
            operands = new Stack<double>();
            operators = new Stack<string>();

            operators.Push(Token.SENTINEL);
            token = Token.NONE;
            tokenPos = -1;
        }

        public double Evaluate(string toEval) {
            Clear();
            LoadConstants();

            expression = toEval;

            if (Normalize(ref expression)) {
                double result = Parse();
                SetVariable(ANSWER_VAR, result);
                return result;
            }

            ThrowException("Blank input expression.");
            return 0;
        }

        private double Parse() {
            ParseBinary();
            Expect(Token.END);
            return operands.Peek();
        }

        private void ParseBinary() {
            ParsePrimary();

            while (Token.IsBinary(token)) {
                PushOperator(token);
                NextToken();
                ParsePrimary();
            }

            while (operators.Peek() != Token.SENTINEL)
                PopOperator();
        }

        private void ParsePrimary() {
            while (true) {
                if (Token.IsDigit(token)) {
                    ParseDigit();
                } else if (Token.IsName(token)) {
                    ParseName();
                } else if (Token.IsUnary(token)) {
                    PushOperator(Token.ConvertOperator(token));
                    NextToken();
                    continue;
                } else {
                    switch (token) {
                        case Token.P_LEFT:
                            NextToken();
                            operators.Push(Token.SENTINEL);
                            ParseBinary();
                            Expect(Token.P_RIGHT, Token.SEPERATOR);
                            operators.Pop();

                            TryInsertMultiply();
                            TryRightSideOperator();
                            break;
                        case Token.SEPERATOR:
                            NextToken();
                            continue;
                        default:
                            ThrowException("Syntax error.");
                            break;
                    }
                }
                break;
            }
        }

        private void ParseDigit() {
            StringBuilder tmpNumber = new StringBuilder();

            while (Token.IsDigit(token))
                CollectToken(ref tmpNumber);

            operands.Push(double.Parse(tmpNumber.ToString(), CultureInfo.InvariantCulture));
            TryInsertMultiply();
            TryRightSideOperator();
        }

        private void ParseName() {
            StringBuilder tmpName = new StringBuilder();

            while (Token.IsName(token))
                CollectToken(ref tmpName);

            string name = tmpName.ToString();

            if (Token.IsFunction(name)) {
                PushOperator(name);
                ParsePrimary();
            } else if (token.Equals(Token.STORE)) {
                NextToken();
                SetVariable(name, Parse());
            } else {
                operands.Push(GetVariable(name));
                TryInsertMultiply();
                TryRightSideOperator();
            }
        }

        private void TryInsertMultiply() {
            if (Token.IsBinary(token) ||
                Token.IsSpecial(token) ||
                Token.IsRightSide(token))
                return;
            PushOperator(Token.MULTIPLY);
            ParsePrimary();
        }

        private void TryRightSideOperator() {
            switch (token) {
                case Token.FACTORIAL:
                    PushOperator(Token.FACTORIAL);
                    NextToken();
                    TryInsertMultiply();
                    break;
                case Token.SEPERATOR:
                    ParsePrimary();
                    break;
            }
        }

        private void PushOperator(string op) {
            if (Token.UNARY_MINUS.Equals(op)) {
                operators.Push(op);
                return;
            }

            while (Token.Compare(operators.Peek(), op) > 0)
                PopOperator();

            operators.Push(op);
        }

        private void PopOperator() {
            if (Token.IsBinary(operators.Peek())) {
                double o2 = operands.Pop();
                double o1 = operands.Pop();
                Calculate(operators.Pop(), o1, o2);
            } else {
                Calculate(operators.Pop(), operands.Pop());
            }
        }

        private void NextToken() {
            if (token != Token.END)
                token = expression[++tokenPos].ToString();
        }

        private void CollectToken(ref StringBuilder sb) {
            sb.Append(token);
            NextToken();
        }

        private void Expect(params string[] expectedTokens) {
            if (expectedTokens.Any(t => token.Equals(t))) {
                NextToken();
                return;
            }

            ThrowException($"Syntax error: {Token.ToString(expectedTokens[0])} expected.");
        }

        private bool Normalize(ref string s) {
            s = s.Replace(" ", "").Replace("\t", " ").ToLower() + Token.END;

            if (s.Length < 2)
                return false;

            NextToken();
            return true;
        }

        private void ThrowException(string message) {
            throw new CalculateException(message, tokenPos);
        }
    }

    [Serializable]
    public class CalculateException : Exception {
        public CalculateException(string message, int position) : base($"Error at position: {position}, {message}") {
            TokenPosition = position;
        }

        public int TokenPosition { get; }
    }
}