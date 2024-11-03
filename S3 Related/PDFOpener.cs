using UnityEngine;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Collections.Generic;

public class PDFOpener : MonoBehaviour
{
    public string pdfFileName = "book.pdf";
    public int currentPage = 0;
    private List<Texture2D> pdfPages = new List<Texture2D>();
    private Renderer pdfRenderer;

    void Start()
    {
        pdfRenderer = GetComponent<Renderer>();
        LoadPDF();
        DisplayPage(currentPage);
    }

    void LoadPDF()
    {
        string pdfPath = Path.Combine(Application.dataPath, pdfFileName);
        using (PdfReader reader = new PdfReader(pdfPath))
        {
            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                Texture2D pageTexture = ExtractPageAsTexture(reader, i);
                pdfPages.Add(pageTexture);
            }
        }
    }

    Texture2D ExtractPageAsTexture(PdfReader reader, int pageNumber)
    {
        Rectangle pageSize = reader.GetPageSize(pageNumber);
        Texture2D texture = new Texture2D((int)pageSize.Width, (int)pageSize.Height);
        
        PdfDictionary pageDictionary = reader.GetPageN(pageNumber);
        PdfDictionary resourcesDictionary = pageDictionary.GetAsDict(PdfName.RESOURCES);
        PdfDictionary xObjectDictionary = resourcesDictionary.GetAsDict(PdfName.XOBJECT);
        
        if (xObjectDictionary != null)
        {
            foreach (PdfName name in xObjectDictionary.Keys)
            {
                PdfObject obj = xObjectDictionary.Get(name);
                if (obj.IsIndirect())
                {
                    PdfDictionary tg = (PdfDictionary)PdfReader.GetPdfObject(obj);
                    PdfName subtype = (PdfName)tg.Get(PdfName.SUBTYPE);
                    if (PdfName.IMAGE.Equals(subtype))
                    {
                        int XrefIndex = ((PRIndirectReference)obj).Number;
                        PdfObject pdfObj = reader.GetPdfObject(XrefIndex);
                        PdfStream pdfStrem = (PdfStream)pdfObj;
                        byte[] bytes = PdfReader.GetStreamBytesRaw((PRStream)pdfStrem);
                        texture.LoadImage(bytes);
                        break;
                    }
                }
            }
        }
        
        return texture;
    }

    void DisplayPage(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < pdfPages.Count)
        {
            pdfRenderer.material.mainTexture = pdfPages[pageIndex];
        }
    }

    public void NextPage()
    {
        if (currentPage < pdfPages.Count - 1)
        {
            currentPage++;
            DisplayPage(currentPage);
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            DisplayPage(currentPage);
        }
    }
}