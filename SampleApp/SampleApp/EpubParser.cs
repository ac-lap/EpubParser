using EpubReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SampleApp
{
    class EpubParser
    {
        public StorageFile file;

        private Epub document;
        private int _pageCount;
        private int _curPageNum;

        private List<string> textList = new List<string>();
        private List<List<byte>> formatList = new List<List<byte>>();

        public EpubParser(StorageFile _file)
        {
            file = _file;
        }

        public async Task loadFile()
        {
            Stream stream = await file.OpenStreamForReadAsync();
            document = new Epub(stream);
            _pageCount = document.Content.Count;
            _curPageNum = -1;

            ProcessEpub();
        }

        private void ProcessEpub()
        {
            for (int i = 0; i < document.Content.Count; i++)
            {
                Tuple<string, List<byte>> content = getText(document.Content[i] as ContentData);
                int prevPos = 0;
                string text = "";
                int pos;

                do
                {
                    try
                    {
                        pos = content.Item1.IndexOf('\n', prevPos + 4000);
                        if (pos == -1)
                        {
                            pos = content.Item1.Length - 1;
                        }
                    }
                    catch
                    {
                        pos = content.Item1.Length - 1;
                    }
                    text = content.Item1.Substring(prevPos, pos - prevPos + 1);

                    if (text.Trim() != "")
                    {
                        textList.Add(text);
                        formatList.Add(content.Item2.GetRange(prevPos, pos - prevPos + 1));
                    }

                    prevPos += text.Length - 1;
                } while (prevPos < content.Item1.Length - 1);
            }
            _pageCount = textList.Count;
        }
        public bool isPageInRange(int pageNum)
        {
            if (pageNum >= 0 && pageNum < _pageCount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public int pageCount()
        {
            return _pageCount;
        }
        public int curPageNum()
        {
            return _curPageNum;
        }
        public Tuple<string, List<byte>> getTextOfNextPage()
        {
            if (_curPageNum < _pageCount - 1)
            {
                return getText_internal(++_curPageNum);
            }
            return null;
        }
        public Tuple<string, List<byte>> getTextOfPage(int pageNum)
        {
            if (pageNum >= 0 && pageNum < _pageCount)
            {
                _curPageNum = pageNum;
                return getText_internal(_curPageNum);
            }
            return null;
        }
        public Tuple<string, List<byte>> getTextofPrevPage()
        {
            if (_curPageNum > 0)
            {
                return getText_internal(--_curPageNum);
            }
            return null;
        }
        public string getJustText(int pageNum)
        {
            if (pageNum >= 0 && pageNum < _pageCount)
            {
                return getText_internal(pageNum).Item1;
            }
            return null;
        }

        public string getExtension()
        {
            return "epub";
        }

        public void Destroy()
        {
            file = null;
            document = null;
        }

        private Tuple<string, List<byte>> getText(ContentData content)
        {
            return content.GetContentAsFormattedText();
        }

        private Tuple<string, List<byte>> getText_internal(int pagenum)
        {
            return new Tuple<string, List<byte>>(textList[pagenum], formatList[pagenum]);
        }
    }
}
