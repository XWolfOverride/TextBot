//MIT License
//
//Copyright(c) 2018 XWolf Override
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TextBot
{
    static class Processor
    {
        public delegate void SimpleDelegate();
        public delegate void PutStringDelegate(string s);
        public delegate string GetStringDelegate();

        // Output management
        static public SimpleDelegate doClear;
        static public PutStringDelegate doWrite;
        static public PutStringDelegate doSetOutput;

        // Input management
        static public SimpleDelegate doClearInput;
        static public PutStringDelegate doSetInput;
        static public PutStringDelegate doWriteInput;
        static public GetStringDelegate doReadInput;

        static internal void WriteLine(string s)
        {
            doWrite?.Invoke(s);
            doWrite?.Invoke("\r\n");
        }

        static public void Execute(string code)
        {
            doClear?.Invoke();
            try
            {
                //if (!code.StartsWith("//@Pure"))
                //{
                //    StringBuilder ncode = new StringBuilder("using System;using TextBot;");
                //    string[] cc = code.Split('\r', '\n');
                //    for (int i = 0; i < cc.Length; i++)
                //    {
                //        string line = cc[i].Trim();
                //        if (line.StartsWith("//@"))
                //            ncode.Append(line.Substring(3));
                //    }
                //    ncode.Append("namespace Generated{public class GeneratedClass:ProcessorBaseClass{");
                //    for (int i = 0; i < cc.Length; i++)
                //    {
                //        string line = cc[i].Trim();
                //        if (line.StartsWith("//#"))
                //            ncode.Append(line.Substring(3));
                //    }
                //    ncode.Append("public override void Process(){\r\n");
                //    ncode.Append(code);
                //    ncode.Append("}}}");
                //    code = ncode.ToString();
                //}

                Assembly asm = BuildAssembly(code);
                ProcessorBaseClass instance = asm.CreateInstance("Generated.GeneratedClass") as ProcessorBaseClass;
                instance.Process();
                instance.flush();
            }
            catch (Exception e)
            {
                WriteLine("[!!]" + e.GetType().Name + ": ");
                WriteLine(e.Message);
                WriteLine(e.StackTrace);
            }
        }

        static private Assembly BuildAssembly(string code)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters compilerparams = new CompilerParameters();
            compilerparams.GenerateExecutable = false;
            compilerparams.GenerateInMemory = true;
            compilerparams.ReferencedAssemblies.Add(Assembly.GetEntryAssembly().Location);
            CompilerResults results = provider.CompileAssemblyFromSource(compilerparams, code);
            if (results.Errors.HasErrors)
            {
                StringBuilder errors = new StringBuilder("Compiler Errors :\r\n");
                foreach (CompilerError error in results.Errors)
                {
                    errors.AppendFormat("Line {0},{1}\t: {2}\n", error.Line, error.Column, error.ErrorText);
                }
                throw new Exception(errors.ToString());
            }
            else
            {
                return results.CompiledAssembly;
            }
        }
    }

    #region ProcessorBaseClass
    public abstract class ProcessorBaseClass
    {
        private StringBuilder output = new StringBuilder();

        protected bool AutoFlush = false;

        public void flush()
        {
            Processor.doSetOutput(output.ToString());
        }

        #region Write Output
        protected void Clear()
        {
            output.Clear();
            Processor.doClear?.Invoke();
        }

        protected void Write(string s)
        {
            if (s == null)
                return;
            output.Append(s);
            if (AutoFlush)
                Processor.doWrite?.Invoke(s);
        }

        protected void Write(params object[] os)
        {
            foreach (object o in os)
                if (o != null)
                    Write(o.ToString());
        }

        protected void WriteLine(string s)
        {
            Write(s);
            WriteNewLine();
        }

        protected void WriteLine(params object[] os)
        {
            foreach (object o in os)
                if (o != null)
                    Write(o.ToString());
            WriteNewLine();
        }

        protected void WriteNewLine()
        {
            Write("\r\n");
        }
        #endregion

        #region Write Input
        protected void SetInput(string s)
        {
            Processor.doSetInput?.Invoke(s);
        }

        protected void ClearInput()
        {
            Processor.doClearInput?.Invoke();
        }

        protected void WriteInput(string s)
        {
            Processor.doWriteInput?.Invoke(s);
        }
        #endregion

        #region Utils
        protected string WGet(string url)
        {
            WebClient client = new WebClient();
            client.Encoding = Encoding.UTF8;
            return client.DownloadString(url);
        }

        protected string NormalizeLines(string data)
        {
            return data.Replace("\r\n", "\n").Replace('\r', '\n').Replace("\n", "\r\n");
        }
        #endregion

        #region Read Input
        protected string GetString()
        {
            string s = Processor.doReadInput?.Invoke();
            if (s == null)
                return "";
            return s;
        }

        protected string[] GetLines()
        {
            return GetString().Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        }
        #endregion

        public abstract void Process();
    }
    #endregion
}
