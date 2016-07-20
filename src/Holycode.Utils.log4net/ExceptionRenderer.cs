using System;
using System.IO;
using log4net.ObjectRenderer;

namespace log4net
{
    internal class ExceptionRenderer : IObjectRenderer
    {
        public int? InnerExceptionLevel = null;
        public bool ShowStackTrace = true;

        public void RenderObject(RendererMap rendererMap, object obj, TextWriter writer)
        {
            if (!(obj is Exception)) return;
            var ex = obj as Exception;
            var curLevel = 0;

            while (ex != null && (InnerExceptionLevel == null || InnerExceptionLevel.Value >= curLevel))
            {
                writer.WriteLine(ex.Message);
                if (ShowStackTrace)
                {
                    writer.WriteLine(ex.StackTrace);
                }
                ex = ex.InnerException;
                if (ex != null) writer.WriteLine("--- Inner Exception:");
                curLevel++;
            }
        }
    }
}