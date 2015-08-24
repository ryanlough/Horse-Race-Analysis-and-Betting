﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Threading;


namespace HorseRacing
{
  /**
   * Main class
   */
  class Program
  {
    public static int currentRace = 0;
    public static string[] sbArray = new string[11];
    static void Main(string[] args)
    {
      DateTime dateTime = new DateTime(2005, 8, 13);

      string zeroMonth = dateTime.Month < 10 ? "0" : "";
      string zeroDay   = dateTime.Day < 10 ? "0" : "";

      string date = zeroMonth + dateTime.Month + "/" + zeroDay + dateTime.Day + "/" + dateTime.Year;
      
      //PdfReader reader = new PdfReader(@"http://www.equibase.com/premium/eqbPDFChartPlus.cfm?RACE=A&BorP=P&TID=SAR&CTRY=USA&DT=" + date + "&DAY=D&STYLE=EQB");
      PdfReader reader = new PdfReader(@"C:\Users\Ryan\test.pdf");
      StringBuilder builder = new StringBuilder();

      for (int x = 1; x <= reader.NumberOfPages; x++)
      {
        PdfDictionary page = reader.GetPageN(x);
        IRenderListener listener = new SBTextRenderer(builder);
        PdfContentStreamProcessor processor = new PdfContentStreamProcessor(listener);
        PdfDictionary pageDic = reader.GetPageN(x);
        PdfDictionary resourcesDic = pageDic.GetAsDict(PdfName.RESOURCES);
        processor.ProcessContent(ContentByteUtils.GetContentBytesForPage(reader, x), resourcesDic);
      }

      DataHandler handler = new DataHandler(dateTime, sbArray);
      Thread thread = new Thread(new ThreadStart(handler.extractPdfData));

      thread.Start();
      thread.Join();
      while (true) ;
    }

    /**
     * Class to convert a PDF into usable text.
     */
    public class SBTextRenderer : IRenderListener
    {

      private StringBuilder _builder;
      public SBTextRenderer(StringBuilder builder)
      {
        _builder = builder;
      }
      #region IRenderListener Members

      public void BeginTextBlock()
      {
      }

      public void EndTextBlock()
      {
      }

      public void RenderImage(ImageRenderInfo renderInfo)
      {
      }

      public void RenderText(TextRenderInfo renderInfo)
      {
        _builder.Append(renderInfo.GetText() + " ");
        if (renderInfo.GetText().Equals("Reserved."))
        {
          sbArray[currentRace] = _builder.ToString();
          _builder.Clear();
          currentRace++;
        }
      }

      #endregion
    }

    /**
     * Class to handle reading and storing all the extracted PDF data.
     */
    public class DataHandler
    {
      private DateTime date;
      private string[] pages;

      public DataHandler(DateTime date, string[] pages)
      {
        this.date = date;
        this.pages = pages;
      }

      public void extractPdfData()
      {
        Race[] races = new Race[11];
        int i = 0;

        foreach (String s in sbArray)
        {
          //Abort if the race has too few characters.
          //Workaround due to equibase putting a ton of redundant text in their pdfs for no reason...
          if (s == null || s.Length < 150)
          {
            break;
          }
          races[i++] = new Race(i, Race.getPurse(s), Horse.getHorses(s), Race.getWeather(s), Race.getTrack(s), Race.getLength(s));
        }

        Console.WriteLine(new Day(date, races).ToString());
      }
    }
  }
}