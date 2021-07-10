using System.Text;

namespace HttpClientGenerator.Internals
{
    internal class SourceBuilder
    {
        private readonly StringBuilder _builder = new StringBuilder();
        private int _indent = 0;

        public SourceBuilder() : this(0)
        {
        }

        public SourceBuilder(int indent)
        {
            _indent = indent;
        }

        public void Indent() => _indent++;

        public void Unindent()
        {
            if (_indent > 0) _indent--;
        }

        public void AppendLine() => AppendLine(string.Empty);

        public void AppendError(string message) => AppendLine($"#error {message}");

        public void AppendLine(string code)
        {
            _builder.AppendLine(IndentedCode(code));
        }

        public void Append(string code)
        {
            _builder.Append(code);
        }

        private string IndentedCode(string code) => new string(' ', 4 * _indent) + code;

        public void OpenBraket()
        {
            AppendLine("{");
            Indent();
        }

        public void CloseBraket()
        {
            Unindent();
            AppendLine("}");
        }

        public void RawAppendLine(string code)
        {
            _builder.AppendLine(code);
        }

        public override string ToString() => _builder.ToString();
    }
}
