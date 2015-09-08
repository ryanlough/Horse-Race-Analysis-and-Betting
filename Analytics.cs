﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HorseRacing
{
  class Analytics
  {
    public delegate Horse Integrand(Race r);

    static void Main(string[] args)
    {
      SQLiteConnection m_dbConnection = new SQLiteConnection("Data Source=Saratoga.sqlite;Version=3;");
      m_dbConnection.Open();

      List<Day> days = DataAccessObject.retrieve(m_dbConnection);


      foreach (Day day in days)
      {
        if (day != null)
          day.setAllHorseRanks();
      }

      findBestBet(days);
    }

    public static void findBestBet(List<Day> days)
    {
      Console.WriteLine("Horses to win:");
      retrieveListForPosn(days, getWin);
      Console.WriteLine("\nHorses to place:");
      retrieveListForPosn(days, getPlace);
      Console.WriteLine("\nHorses to show:");
      retrieveListForPosn(days, getShow);
      Console.WriteLine("\nHorses to fourth:");
      retrieveListForPosn(days, getFourth);

      Console.WriteLine("\nExacta:");
      bestExacta(days);

      Console.ReadKey();
    }

    public static void retrieveListForPosn(List<Day> days, Integrand f) {
      Dictionary<byte, int> result = new Dictionary<byte, int>();
      

      foreach (Day day in days)
      {
        if (day != null && day.getRaces() != null) {
          foreach (Race race in day.getRaces())
          {
            byte rank = f(race).getOddRank();
            if (!result.ContainsKey(rank))
            {
              result.Add(rank, 0);
            }
            else
            {
              result[rank] += 1;
            }
          }
        }
      }

      foreach (KeyValuePair<byte, int> kvp in from entry in result orderby entry.Value descending select entry)
      {
        Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
      }
    }

    public static void bestExacta(List<Day> days)
    {
      Dictionary<byte[], int> result = new Dictionary<byte[], int>(new ByteArrayComparer());
      foreach (Day day in days)
      {
        if (day != null && day.getRaces() != null)
        {
          foreach (Race race in day.getRaces())
          {
            byte[] winners = exactaHelper(race);
            if (!result.ContainsKey(winners))
            {
              result.Add(winners, 0);
            }
            else
            {
              result[winners] += 1;
            }
          }
        }
      }

      foreach (KeyValuePair<byte[], int> kvp in from entry in result orderby entry.Value descending select entry)
      {
        Console.WriteLine("Key = {0},{1}, Value = {2}", kvp.Key[0], kvp.Key[1], kvp.Value); Console.ReadKey();
      }
    }

    private static byte[] exactaHelper(Race race)
    {
      return new byte[] { getWin(race).getOddRank(), getPlace(race).getOddRank() };
    }

    private static Horse getWin(Race r)
    {
      return r.getWin();
    }

    private static Horse getPlace(Race r)
    {
      return r.getPlace();
    }

    private static Horse getShow(Race r)
    {
      return r.getShow();
    }

    private static Horse getFourth(Race r)
    {
      return r.getFourth();
    }

    private class ByteArrayComparer : IEqualityComparer<byte[]>
    {
      public int GetHashCode(byte[] obj)
      {
        byte[] arr = obj as byte[];
        int hash = 0;
        foreach (byte b in arr) hash ^= b;
        return hash;
      }
      public new bool Equals(byte[] x, byte[] y)
      {
        byte[] arr1 = x as byte[];
        byte[] arr2 = y as byte[];
        if (arr1.Length != arr2.Length) return false;
        for (int ix = 0; ix < arr1.Length; ++ix)
          if (arr1[ix] != arr2[ix]) return false;
        return true;
      }
    }
  }
}