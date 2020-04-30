using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SolitaireChess
{
    public class SolitaireChessDAO
    {
        private SolitaireChessContext db;
        private SolitaireChessPersistenceContext dbc;
        private LinkedList<int> easy = new LinkedList<int>();
        private LinkedList<int> medium = new LinkedList<int>();
        private LinkedList<int> hard = new LinkedList<int>();
        private LinkedList<int> expert = new LinkedList<int>();

        public SolitaireChessDAO(SolitaireChessContext c1, SolitaireChessPersistenceContext c2)
        {
            db = c1;
            db.ZJSMONODE.Load();
            dbc = c2;

            int[] easyArray = new int[100] { 58, 359, 188, 16, 207, 206, 21, 319, 138, 361, 190, 23, 262, 53, 285, 140, 369, 200, 31, 259, 116, 348, 173, 5, 242, 75, 306, 128, 393, 226, 284, 163, 394, 265, 35, 267, 39, 199, 30, 258, 88, 322, 142, 373, 204, 34, 182, 15, 136, 366, 197, 28, 257, 376, 209, 36, 264, 92, 327, 146, 268, 94, 329, 378, 211, 37, 270, 97, 331, 154, 387, 219, 52, 282, 106, 339, 166, 397, 229, 60, 292, 114, 346, 151, 171, 3, 236, 69, 300, 123, 353, 179, 11, 384, 217, 46, 278, 103, 336, 160 };
            int[] mediumArray = new int[100] { 392, 96, 246, 73, 310, 324, 164, 1, 227, 50, 272, 93, 333, 148, 380, 213, 42, 274, 101, 147, 48, 280, 104, 338, 165, 396, 228, 59, 290, 249, 80, 312, 113, 286, 215, 44, 276, 54, 287, 108, 342, 168, 400, 234, 65, 297, 184, 120, 18, 250, 81, 314, 121, 371, 202, 32, 261, 90, 238, 71, 304, 125, 303, 126, 354, 180, 13, 247, 79, 311, 131, 360, 189, 22, 253, 86, 320, 76, 307, 129, 356, 183, 17, 139, 150, 382, 216, 45, 277, 102, 335, 159, 390, 224, 55, 288, 110, 345, 169, 368 };
            int[] hardArray = new int[100] { 201, 244, 68, 57, 298, 111, 337, 170, 395, 222, 40, 266, 99, 328, 149, 381, 214, 233, 43, 275, 132, 362, 192, 25, 64, 296, 118, 350, 175, 7, 240, 74, 255, 187, 20, 252, 85, 318, 137, 367, 198, 29, 232, 63, 295, 117, 349, 174, 6, 239, 72, 305, 127, 355, 181, 14, 47, 375, 208, 289, 112, 399, 231, 62, 294, 119, 313, 133, 363, 193, 379, 212, 41, 273, 100, 334, 157, 389, 223, 383, 191, 109, 343, 357, 158, 391, 225, 56, 260, 89, 323, 143, 374, 205, 351, 177, 8, 243, 370, 77 };
            int[] expertArray = new int[100] { 308, 359, 188, 16, 207, 206, 21, 319, 138, 361, 190, 23, 262, 53, 285, 140, 369, 200, 31, 259, 116, 348, 173, 5, 242, 75, 306, 128, 393, 226, 284, 163, 394, 265, 35, 267, 39, 199, 30, 258, 88, 322, 142, 373, 204, 34, 182, 15, 136, 366, 197, 28, 257, 376, 209, 36, 264, 92, 327, 146, 268, 94, 329, 378, 211, 37, 270, 97, 331, 154, 387, 219, 52, 282, 106, 339, 166, 397, 229, 60, 292, 114, 346, 151, 171, 3, 236, 69, 300, 123, 353, 179, 11, 384, 217, 46, 278, 103, 336, 160 };
            easy = new LinkedList<int>(easyArray);
            medium = new LinkedList<int>(mediumArray);
            hard = new LinkedList<int>(hardArray);
            expert = new LinkedList<int>(expertArray);
           
        }

        private List<ZJSMONODE> GetFirstPuzzlesFromPresupplied()
        {
            var n = from b in db.ZJSMONODE
                    join l in db.ZJSMOLLCONTAINER on b.ZCONTAINER equals l.Z_PK
                    where b.ZPREV == null
                    orderby l.ZCHALLENGELEVEL ascending
                    select b;

            return n.ToList();
        }

        internal ZJSMONODE GetPuzzleByLevelAndPK(int v, int i)
        {
            ZJSMONODE n = new ZJSMONODE();
            switch(v)
            {
                case 0:
                    n =db.Find(n.GetType(), new object[] { easy.ElementAt(i-1) }) as ZJSMONODE;
                    break;
                case 1:
                    n = db.Find(n.GetType(), new object[] { medium.ElementAt(i-1) }) as ZJSMONODE;
                    break;
                case 2:
                    n = db.Find(n.GetType(), new object[] { hard.ElementAt(i-1) }) as ZJSMONODE;
                    break;
                case 3:
                    n = db.Find(n.GetType(), new object[] { expert.ElementAt(i-1) }) as ZJSMONODE;
                    break;

            }
            return n;
        }

        internal ZJSMONODE GetNext(int selectedPuzzle)
        {
            ZJSMONODE n = new ZJSMONODE();
            n = db.Find(n.GetType(), new object[] { selectedPuzzle }) as ZJSMONODE;
            if(n.ZNEXT != null)
            {
                int? pk = n.ZNEXT;
                n = db.Find(n.GetType(), new object[] { pk }) as ZJSMONODE;
                return n;
            }
            else
            {
                return null;
            }
        }
    }


    public class SolitaireChessContext : DbContext
    {
        public DbSet<ZJSMONODE> ZJSMONODE { get; set; }
        public DbSet<ZJSMOLLCONTAINER> ZJSMOLLCONTAINER { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string path = Windows.ApplicationModel.Package.Current.InstalledLocation.Path + "\\Assets\\SolChess.thinkfun";
            optionsBuilder.UseSqlite("Filename=" + path );
        }
    }

    public class SolitaireChessPersistenceContext : DbContext
    {
        public DbSet<SolitaireChessConfig> SolitaireChessConfig { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=persistence.sqlite");
        }
    }

    public class SolitaireChessConfig
    {
        [Key]
        public int PK { get; set; }
        public string Name { get; set; }
        public int Theme { get; set; }
    }

    public class ZJSMONODE
    {
        [Key]
        public int Z_PK { get; set; }
        public int? Z_ENT { get; set; }
        public int? Z_OPT { get; set; }
        public int? ZCONTAINER { get; set; }
        public int? Z2_CONTAINER { get; set; }
        public int? ZNEXT { get; set; }
        public int? Z4_NEXT { get; set; }
        public int? ZPREV { get; set; }
        public int? Z4_PREV { get; set; }
        public int? ZCHALLENGEID { get; set; }
        public int? ZCOMPLETED { get; set; }
        public int? ZUSEDHINTS { get; set; }
        public string ZINITGRID { get; set; }
        public string ZSAVEDGRID { get; set; }
        public string ZSAVEDMOVES { get; set; }
    }

    public class ZJSMOLLCONTAINER
    {
        [Key]
        public int Z_PK { get; set; }
        public int? Z_ENT { get; set; }
        public int? Z_OPT { get; set; }
        public int? ZCHALLENGELEVEL { get; set; }
        public int? ZLASTPLAYED { get; set; }
        public int? ZCHALLENGEPACK { get; set; } 
    }

    class Move
    {
        public int r1, c1, r2, c2, sc;
        public char captured, capturer;
        public Move previous, next, sibling;

        public Move()
        {
            previous = next = sibling = null;
            r1 = c1 = r2 = c2 = sc = 0;
            captured = ' ';
        }

        public void PrintBoard(Puzzle board)
        {
            for (int a = 0; a < Puzzle.SIZE; a++)
            {
                for (int b = 0; b < Puzzle.SIZE; b++)
                {
                    Console.WriteLine("%c|", board.board[a][b].piece);
                }
                Console.WriteLine("\n");
            }
            Console.WriteLine("\n");
        }

        public Move DoMove(Puzzle b, int rFrom, int cFrom, int rTo, int cTo)
        {
            r1 = rFrom;
            c1 = cFrom;
            r2 = rTo;
            c2 = cTo;
            //printf("Target content: %c\n", b.board[rTo][cTo].piece);
            capturer = b.board[rFrom][cFrom].piece;
            captured = b.board[rTo][cTo].piece;
            if (captured == ' ')
            {
                return null;
            }
            switch (b.board[rFrom][cFrom].piece)
            {
                case 'P':
                    if (rTo >= 0 && rTo == rFrom - 1 && cTo >= 0 && cTo < Puzzle.SIZE)
                    {
                        Field target = null;
                        if (b.board[rFrom][cFrom].ne.c == cTo)
                        {
                            target = b.board[rFrom][cFrom].ne;
                        }
                        else if (b.board[rFrom][cFrom].nw.c == cTo)
                        {
                            target = b.board[rFrom][cFrom].nw;
                        }
                        else if (b.board[rFrom][cFrom].n.c == cTo && b.board[rFrom][cFrom].n.piece == ' ')
                        {
                            target = b.board[rFrom][cFrom].n;
                        }
                        if (target != null)
                        {
                            target.piece = b.board[rFrom][cFrom].piece;
                            b.board[rFrom][cFrom].piece = ' ';
                            return this;
                        }
                        return null;
                    }
                    break;
                case 'p':
                    break;
                case 'n':
                case 'N':
                    if (rTo >= 0 && rTo < Puzzle.SIZE && cTo >= 0 && cTo < Puzzle.SIZE)
                    {
                        Field target = null;
                        if (b.board[rFrom][cFrom].n.n.e.c == cTo && b.board[rFrom][cFrom].n.n.e.r == rTo)
                        {
                            target = b.board[rFrom][cFrom].n.n.e;
                        }
                        else if (b.board[rFrom][cFrom].n.e.e.c == cTo && b.board[rFrom][cFrom].n.e.e.r == rTo)
                        {
                            target = b.board[rFrom][cFrom].n.e.e;
                        }
                        else if (b.board[rFrom][cFrom].n.w.w.c == cTo && b.board[rFrom][cFrom].n.w.w.r == rTo)
                        {
                            target = b.board[rFrom][cFrom].n.w.w;
                        }
                        else if (b.board[rFrom][cFrom].n.n.w.c == cTo && b.board[rFrom][cFrom].n.n.w.r == rTo)
                        {
                            target = b.board[rFrom][cFrom].n.n.w;
                        }
                        else if (b.board[rFrom][cFrom].w.w.n.c == cTo && b.board[rFrom][cFrom].w.w.n.r == rTo)
                        {
                            target = b.board[rFrom][cFrom].w.w.n;
                        }
                        else if (b.board[rFrom][cFrom].w.w.s.c == cTo && b.board[rFrom][cFrom].w.w.s.r == rTo)
                        {
                            target = b.board[rFrom][cFrom].w.w.s;
                        }
                        else if (b.board[rFrom][cFrom].s.s.e.c == cTo && b.board[rFrom][cFrom].s.s.e.r == rTo)
                        {
                            target = b.board[rFrom][cFrom].s.s.e;
                        }
                        else if (b.board[rFrom][cFrom].s.s.w.c == cTo && b.board[rFrom][cFrom].s.s.w.r == rTo)
                        {
                            target = b.board[rFrom][cFrom].s.s.w;
                        }
                        else if (b.board[rFrom][cFrom].w.w.s.c == cTo && b.board[rFrom][cFrom].w.w.s.r == rTo)
                        {
                            target = b.board[rFrom][cFrom].w.w.s;
                        }
                        else if (b.board[rFrom][cFrom].w.w.n.c == cTo && b.board[rFrom][cFrom].w.w.n.r == rTo)
                        {
                            target = b.board[rFrom][cFrom].w.w.n;
                        }
                        else if (b.board[rFrom][cFrom].s.e.e.c == cTo && b.board[rFrom][cFrom].s.e.e.r == rTo)
                        {
                            target = b.board[rFrom][cFrom].s.e.e;
                        }
                        else if (b.board[rFrom][cFrom].s.w.w.c == cTo && b.board[rFrom][cFrom].s.w.w.r == rTo)
                        {
                            target = b.board[rFrom][cFrom].s.w.w;
                        }
                        if (target != null)
                        {
                            target.piece = b.board[rFrom][cFrom].piece;
                            b.board[rFrom][cFrom].piece = ' ';
                            return this;
                        }
                        return null;
                    }
                    break;
                case 'q':
                case 'Q':
                    if (rTo >= 0 && rTo < Puzzle.SIZE && cTo >= 0 && cTo < Puzzle.SIZE)
                    {
                        Field target = null;
                        target = b.board[rFrom][cFrom];
                        if (rTo > rFrom && cTo > cFrom)
                        {
                            while (target.se != b.board[rTo][cTo])
                            {
                                target = target.se;
                                if (target == b.offboard || target.piece != ' ')
                                {
                                    return null;
                                }
                            }
                        }
                        else if (rTo > rFrom && cTo == cFrom)
                        {
                            while (target.s != b.board[rTo][cTo])
                            {
                                target = target.s;
                                if (target == b.offboard || target.piece != ' ')
                                {
                                    return null;
                                }
                            }
                        }
                        else if (rTo == rFrom && cTo > cFrom)
                        {
                            while (target.e != b.board[rTo][cTo])
                            {
                                target = target.e;
                                if (target == b.offboard || target.piece != ' ')
                                {
                                    return null;
                                }
                            }
                        }
                        else if (rTo < rFrom && cTo == cFrom)
                        {
                            while (target.n != b.board[rTo][cTo])
                            {
                                target = target.n;
                                if (target == b.offboard || target.piece != ' ')
                                {
                                    return null;
                                }
                            }
                        }
                        else if (rTo > rFrom && cTo < cFrom)
                        {
                            while (target.sw != b.board[rTo][cTo])
                            {
                                target = target.sw;
                                if (target == b.offboard || target.piece != ' ')
                                {
                                    return null;
                                }

                            }
                        }
                        else if (rTo == rFrom && cTo < cFrom)
                        {
                            while (target.w != b.board[rTo][cTo])
                            {
                                target = target.w;
                                if (target == b.offboard || target.piece != ' ')
                                {
                                    return null;
                                }
                            }
                        }
                        else if (rTo < rFrom && cTo < cFrom)
                        {
                            while (target.nw != b.board[rTo][cTo])
                            {
                                target = target.nw;
                                if (target == b.offboard || target.piece != ' ')
                                {
                                    return null;
                                }
                            }
                        }
                        else if (rTo < rFrom && cTo > cFrom)
                        {
                            while (target.ne != b.board[rTo][cTo])
                            {
                                target = target.ne;
                                if (target == b.offboard || target.piece != ' ')
                                {
                                    return null;
                                }
                            }
                        }
                        target = b.board[rTo][cTo];
                        target.piece = b.board[rFrom][cFrom].piece;
                        b.board[rFrom][cFrom].piece = ' ';
                        return this;
                    }
                    break;
                case 'b':
                case 'B':
                    if (rTo >= 0 && rTo < Puzzle.SIZE && cTo >= 0 && cTo < Puzzle.SIZE)
                    {
                        Field target = null;
                        target = b.board[rFrom][cFrom];
                        if (rTo > rFrom && cTo > cFrom)
                        {
                            while (target.se != b.board[rTo][cTo])
                            {
                                target = target.se;
                                if (target == b.offboard || target.piece != ' ')
                                {
                                    return null;
                                }
                            }
                        }
                        else if (rTo > rFrom && cTo < cFrom)
                        {
                            while (target.sw != b.board[rTo][cTo])
                            {
                                target = target.sw;
                                if (target == b.offboard || target.piece != ' ')
                                {
                                    return null;
                                }

                            }
                        }
                        else if (rTo < rFrom && cTo < cFrom)
                        {
                            while (target.nw != b.board[rTo][cTo])
                            {
                                target = target.nw;
                                if (target == b.offboard || target.piece != ' ')
                                {
                                    return null;
                                }
                            }
                        }
                        else if (rTo < rFrom && cTo > cFrom)
                        {
                            while (target.ne != b.board[rTo][cTo])
                            {
                                target = target.ne;
                                if (target == b.offboard || target.piece != ' ')
                                {
                                    return null;
                                }
                            }
                        }
                        else
                        {
                            return null;
                        }
                        target = b.board[rTo][cTo];
                        target.piece = b.board[rFrom][cFrom].piece;
                        b.board[rFrom][cFrom].piece = ' ';
                        return this;
                    }
                    break;
                case 'r':
                case 'R':
                    if (rTo >= 0 && rTo < Puzzle.SIZE && cTo >= 0 && cTo < Puzzle.SIZE)
                    {
                        Field target = null;
                        target = b.board[rFrom][cFrom];
                        if (rTo > rFrom && cTo == cFrom)
                        {
                            while (target.s != b.board[rTo][cTo])
                            {
                                target = target.s;
                                if (target == b.offboard || target.piece != ' ')
                                {
                                    return null;
                                }
                            }
                        }
                        else if (rTo == rFrom && cTo > cFrom)
                        {
                            while (target.e != b.board[rTo][cTo])
                            {
                                target = target.e;
                                if (target == b.offboard || target.piece != ' ')
                                {
                                    return null;
                                }
                            }
                        }
                        else if (rTo < rFrom && cTo == cFrom)
                        {
                            while (target.n != b.board[rTo][cTo])
                            {
                                target = target.n;
                                if (target == b.offboard || target.piece != ' ')
                                {
                                    return null;
                                }
                            }
                        }
                        else if (rTo == rFrom && cTo < cFrom)
                        {
                            while (target.w != b.board[rTo][cTo])
                            {
                                target = target.w;
                                if (target == b.offboard || target.piece != ' ')
                                {
                                    return null;
                                }
                            }
                        }
                        else
                        {
                            return null;
                        }
                        target = b.board[rTo][cTo];
                        target.piece = b.board[rFrom][cFrom].piece;
                        b.board[rFrom][cFrom].piece = ' ';
                        return this;
                    }
                    break;
                case 'k':
                case 'K':
                    if (rTo >= 0 && rTo < Puzzle.SIZE && cTo >= 0 && cTo < Puzzle.SIZE)
                    {
                        Field target = null;
                        target = b.board[rTo][cTo];
                        if (target != null && (target.ne == b.board[rFrom][cFrom] || target.e == b.board[rFrom][cFrom] || target.se == b.board[rFrom][cFrom] || target.s == b.board[rFrom][cFrom] || target.sw == b.board[rFrom][cFrom] || target.w == b.board[rFrom][cFrom] || target.nw == b.board[rFrom][cFrom] || target.n == b.board[rFrom][cFrom]))
                        {
                            target.piece = b.board[rFrom][cFrom].piece;
                            b.board[rFrom][cFrom].piece = ' ';
                            return this;
                        }
                        return null;

                    }
                    break;
                default:
                    return null;
            }
            return null;
        }

        public void DoMove(Puzzle b)
        {
            DoMove(b, r1, c1, r2, c2);
        }

        internal void UndoMove(Puzzle b)
        {
            b.board[r1][c1].piece = b.board[r2][c2].piece;
            b.board[r2][c2].piece = captured;
        }
    }

    class Field
    {
        public Field n, ne, e, se, s, sw, w, nw;
        public int c, r;
        public char piece;
        public Field()
        {
            n = ne = e = se = s = sw = w = nw = null;
            c = r = -1;
            piece = (char)0;
        }
    }

    class Puzzle
    {
        public static readonly int SIZE = 4;
        public Field offboard;
        public Field[][] board = new Field[SIZE][];

        public int pieceCount { get; set; }

        public Puzzle()
        {
            pieceCount = 0;
            offboard = new Field();
            offboard.n = offboard.ne = offboard.nw = offboard.e = offboard.s = offboard.se = offboard.sw = offboard.w = offboard;
            for (int r = 0; r < SIZE; r++)
            {
                board[r] = new Field[SIZE];
                for (int c = 0; c < SIZE; c++)
                {
                    board[r][c] = new Field();
                    board[r][c].piece = ' ';
                    board[r][c].n = board[r][c].ne = board[r][c].e = board[r][c].se = board[r][c].s = board[r][c].sw = board[r][c].w = board[r][c].nw = offboard;
                    board[r][c].r = r;
                    board[r][c].c = c;
                }
            }
            for (int x = 0; x < SIZE; x++)
            {
                for (int y = 0; y < SIZE; y++)
                {
                    board[x][y].n = x == 0 ? offboard : board[x - 1][y];
                    board[x][y].ne = (x == 0 || y == SIZE - 1) ? offboard : board[x - 1][y + 1];
                    board[x][y].e = y == SIZE - 1 ? offboard : board[x][y + 1];
                    board[x][y].se = (y == SIZE - 1 || x == SIZE - 1) ? offboard : board[x + 1][y + 1];
                    board[x][y].s = x == SIZE - 1 ? offboard : board[x + 1][y];
                    board[x][y].sw = (y == 0 || x == SIZE - 1) ? offboard : board[x + 1][y - 1];
                    board[x][y].w = y == 0 ? offboard : board[x][y - 1];
                    board[x][y].nw = (y == 0 || x == 0) ? offboard : board[x - 1][y - 1];
                }
            }
        }
    }
}
