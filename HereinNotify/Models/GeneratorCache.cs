using Microsoft.CodeAnalysis;
using System.Text;

namespace HereinNotify.Models
{
    internal class GeneratorCache<TClassCache> where TClassCache : ClassCache
    {
        public SourceProductionContext Context { get; }
        public TClassCache ClassCache { get;  }

        private StringBuilder Builder { get; }

        public GeneratorCache(Microsoft.CodeAnalysis.SourceProductionContext context, TClassCache cache, StringBuilder builder)
        {
            Context = context;
            ClassCache = cache;
            Builder = builder;
        }

        /// <summary>
        /// 缩进Tab
        /// </summary>
        private int retractCount = 0;

        /// <summary>
        /// 增加缩进
        /// </summary>
        public void IncreaseTab() => retractCount++;

        /// <summary>
        /// 减少缩进
        /// </summary>
        public void DecreaseTab() => retractCount--;

        /// <summary>
        /// 增加代码
        /// </summary>
        /// <param name="code"></param>
        public void AppendCode(string code)
        {
            var retract = new string(' ', retractCount * 4);
            Builder.Append(retract);
            Builder.AppendLine(code);
        }

        /// <summary>
        /// 生成代码
        /// </summary>
        /// <returns></returns>
        public string ToCode() { return Builder.ToString(); }
    }
}
