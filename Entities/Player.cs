using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpGame.Entities;

public class Player
{
    public int Id { get; set; }
    public int Score = 0;

    static int lastId;

    public Player()
    {
        if (lastId == null)
        {
            Id = 0;
            lastId = 0;
        } else
        {
            Id = ++lastId;
        }

    }
}
