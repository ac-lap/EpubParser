﻿namespace EpubReader
{
  public class DateData
  {
    public string Type { get; private set; }

    public string Date { get; private set; }

    public DateData(string type, string date)
    {
      this.Type = type;
      this.Date = date;
    }
  }
}
