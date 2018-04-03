#region usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

#endregion

namespace Convex.Plugin.Calculator {
    public partial class InlineCalculator {
        private string _expression;

        private Stack<double> _operands;
        private Stack<string> _operators;

        private string _token;
        private int _tokenPos;

        public void Clear() {
            _operands = new Stack<double>();
            _operators = new Stack<string>();

            _operators.Push(Token.SENTINEL);
            _token = Token.NONE;
            _tokenPos = -1;
        }

        public double Evaluate(string toEval) {
            Clear();
            LoadConstants();

            _expression = toEval;

            if (Normalize(ref _expression)) {
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
            return _operands.Peek();
        }

        private void ParseBinary() {
            ParsePrimary();

            while (Token.IsBinary(_token)) {
                PushOperator(_token);
                NextToken();
                ParsePrimary();
            }

            while (_operators.Peek() != Token.SENTINEL)
                PopOperator();
        }

        private void ParsePrimary() {
            while (true) {
                if (Token.IsDigit(_token)) {
                    ParseDigit();
                } else if (Token.IsName(_token)) {
                    ParseName();
                } else if (Token.IsUnary(_token)) {
                    PushOperator(Token.ConvertOperator(_token));
                    NextToken();
                    continue;
                } else {
                    switch (_token) {
                        case Token.P_LEFT:
                            NextToken();
                            _operators.Push(Token.SENTINEL);
                            ParseBinary();
                            Expect(Token.P_RIGHT, Token.SEPERATOR);
                            _operators.Pop();

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

            while (Token.IsDigit(_token))
                CollectToken(ref tmpNumber);

            _operands.Push(double.Parse(tmpNumber.ToString(), CultureInfo.InvariantCulture));
            TryInsertMultiply();
            TryRightSideOperator();
        }

        private void ParseName() {
            StringBuilder tmpName = new StringBuilder();

            while (Token.IsName(_token))
                CollectToken(ref tmpName);

            string name = tmpName.ToString();

            if (Token.IsFunction(name)) {
                PushOperator(name);
                ParsePrimary();
            } else if (_token.Equals(Token.STORE)) {
                NextToken();
                SetVariable(name, Parse());
            } else {
                _operands.Push(GetVariable(name));
                TryInsertMultiply();
                TryRightSideOperator();
            }
        }

        private void TryInsertMultiply() {
            if (Token.IsBinary(_token) ||
                Token.IsSpecial(_token) ||
                Token.IsRightSide(_token))
                return;
            PushOperator(Token.MULTIPLY);
            ParsePrimary();
        }

        private void TryRightSideOperator() {
            switch (_token) {
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
                _operators.Push(op);
                return;
            }

            while (Token.Compare(_operators.Peek(), op) > 0)
                PopOperator();

            _operators.Push(op);
        }

        private void PopOperator() {
            if (Token.IsBinary(_operators.Peek())) {
                double o2 = _operands.Pop();
                double o1 = _operands.Pop();
                Calculate(_operators.Pop(), o1, o2);
            } else {
                Calculate(_operators.Pop(), _operands.Pop());
            }
        }

        private void NextToken() {
            if (_token != Token.END)
                _token = _expression[++_tokenPos].ToString();
        }

        private void CollectToken(ref StringBuilder sb) {
            sb.Append(_token);
            NextToken();
        }

        private void Expect(params string[] expectedTokens) {
            if (expectedTokens.Any(t => _token.Equals(t))) {
                NextToken();
                return;
            }

            ThrowException($"Syntax error: {Token.ToString(expectedTokens[0])} expected.");
        }

        private bool Normalize(ref string s) {
            s = s.Replace(" ", "")
                    .Replace("\t", " ")
                    .ToLower() + Token.END;

            if (s.Length < 2)
                return false;

            NextToken();
            return true;
        }

        private void ThrowException(string message) {
            throw new CalculateException(message, _tokenPos);
        }
    }

    public class CalculateException : Exception {
        public CalculateException(string message, int position) : base($"Error at position: {position}, {message}") {
            TokenPosition = position;
        }

        public int TokenPosition { get; }
    }
}