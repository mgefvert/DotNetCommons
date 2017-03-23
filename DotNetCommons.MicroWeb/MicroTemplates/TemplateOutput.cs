using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCommons.MicroWeb.MicroTemplates
{
    public class TemplateOutput
    {
        public class TemplateOutputBlock
        {
            public string Name { get; }
            public ArrayList Lines { get; } = new ArrayList(256);

            public TemplateOutputBlock(string name)
            {
                Name = name;
            }
        }

        private readonly List<TemplateOutputBlock> _blockList = new List<TemplateOutputBlock>();
        private readonly Dictionary<string, TemplateOutputBlock> _blockIndex = new Dictionary<string, TemplateOutputBlock>();
        private TemplateOutputBlock _currentBlock;
        public string ExtendsTemplate { get; set; }
        public string TemplateFilename { get; }

        public TemplateOutput(string templateFilename)
        {
            TemplateFilename = templateFilename;
        }

        public void Add(string text)
        {
            if (_currentBlock == null)
                BlockStart(null);

            _currentBlock?.Lines.Add(text);
        }

        public void BlockStart(string block)
        {
            if (string.IsNullOrEmpty(block))
            {
                _blockList.Add(_currentBlock = new TemplateOutputBlock(block));
                return;
            }

            if (!_blockIndex.ContainsKey(block))
            {
                _currentBlock = new TemplateOutputBlock(block);
                _blockList.Add(_currentBlock);
                _blockIndex.Add(block, _currentBlock);
            }
            else
                _currentBlock = _blockIndex[block];
        }

        public void BlockEnd()
        {
            _currentBlock = null;
        }

        public TemplateOutputBlock GetBlock(string name)
        {
            TemplateOutputBlock result;
            return _blockIndex.TryGetValue(name, out result) ? result : null;
        }

        public string GetResult()
        {
            return string.Join("\r\n", _blockList.SelectMany(x => x.Lines.ToArray()));
        }

        public static TemplateOutput Merge(TemplateOutput master, TemplateOutput template)
        {
            var result = new TemplateOutput(master.TemplateFilename)
            {
                ExtendsTemplate = master.ExtendsTemplate
            };

            foreach (var block in master._blockList)
            {
                if (string.IsNullOrEmpty(block.Name))
                    result._blockList.Add(block);
                else
                    result._blockList.Add(template.GetBlock(block.Name) ?? block);
            }

            return result;
        }
    }
}
