using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SolitaireChess
{
    public class Generator
    {
        ulong one;
        string b = "-----------------";
        bool run;

        string Set = "KQRRNNBBPPPPPPPP----------------";
        public Generator()
        { 
            one = 1;
            run = false;
        }

        public string Generate()
        {
            run = true;
            b = "----------------";
            Comb(32, 16, 0, 0);
            return b;
        }

        public void Comb(int pool, int need, ulong chosen, int at)
        {
            if (!run || (pool < need + at)) return; /* not enough bits left */

            if (need == 0 && run)
            {
                /* got all we needed; print the thing.  if other actions are
                 * desired, we could have passed in a callback function. */
                int k = 0;
                for (at = 0; at < pool; at++)
                {
                    if ((chosen & (one << at)) > 0)
                    {
                        StringBuilder sb = new StringBuilder(b);
                        sb[k] = Set[at];
                        b = sb.ToString();
                        k++;
                    }
                }
                if (b.Equals("----------------"))
                {
                    Solver s = new Solver();
                    if (s.InitAndRun(b) > 0)
                    {
                        run = false;
                    }
                }
                return;
            }
            /* if we choose the current item, "or" (|) the bit to mark it so. */
            if (run)
            {
                Comb(pool, need - 1, chosen | (one << at), at + 1);
                Comb(pool, need, chosen, at + 1);  /* or don't choose it, go to next */
            }
        }
    }


    public class Controller
    {
        private int selectedPuzzle, currentLevel;
        private static Controller Instance;
        SolitaireChessDAO dao;

        public static Controller GetInstance()
        {
            if(Instance == null)
            {
                Instance = new Controller();
            }
            return Instance;
        }

        public void OpenDatabases()
        {
            dao = new SolitaireChessDAO(new SolitaireChessContext(), new SolitaireChessPersistenceContext());
        }

        private Controller()
        {
            
        }


        public void RetrievePuzzleFromPresupplied(int level, int number)
        { 
        }

        public bool SelectNextPuzzle()
        {
            SaveFinished(selectedPuzzle);
            if(selectedPuzzle < 100)
            {
                selectedPuzzle++;
                NavigateTo(currentLevel, selectedPuzzle);
                return true;
            }
            return false;
        }

        private void SaveFinished(int selectedPuzzle)
        {
            
        }

        internal void NavigateTo(int v, int i)
        {
            PUZZLE puzzle = dao.GetPuzzleByLevelAndPK(v, i);
            if(puzzle != null)
            {
                selectedPuzzle = i;
                currentLevel = v;
                var frame = (Frame)Window.Current.Content;
                frame.Navigate(typeof(Board));
                Board b = (Board)frame.Content;
                b.SetPuzzle(puzzle.GRID);
            }

        }

        internal bool IsFinished(int v, int i)
        {
            PUZZLE puzzle = dao.GetPuzzleByLevelAndPK(v, i);
            return puzzle != null && puzzle.COMPLETED != null;
        }
    }

    
    

    class Solver
    {
        int nSol = 0;
        public Move root { get; internal set; }

        public int Solve(Puzzle b, Move move)
        {
            if (b.pieceCount == 1 && nSol == 0)
            {
                root = move;
                nSol++;
                return 1;
            }
            //if(b.pieceCount == 2) move.printBoard(b);
            for (int r = 0; r < Puzzle.SIZE; r++)
            {
                for (int c = 0; c < Puzzle.SIZE; c++)
                {
                    if (b.board[r][c].piece != ' ')
                    {
                        for (int x = 0; x < Puzzle.SIZE; x++)
                        {
                            for (int y = 0; y < Puzzle.SIZE; y++)
                            {
                                if (b.board[x][y].piece != ' ' && b.board[r][c] != b.board[x][y])
                                {
                                    Move m = new Move();
                                    if (b.pieceCount > 1 && nSol == 0 && m.DoMove(b, r, c, x, y) != null)
                                    {
                                        move.next = m;
                                        m.previous = move;
                                        b.pieceCount--;
                                        Solve(b, m);
                                        m.UndoMove(b);
                                        b.pieceCount++;
                                    }
                                    else
                                    {
                                        
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return 0;
        }

        public int InitAndRun(string b)
        {
            Puzzle board = new Puzzle();
            root = new Move();
            if (b.Length >= 16)
            {
                for (int i = 0; i < Puzzle.SIZE; i++)
                {
                    for (int j = 0; j < Puzzle.SIZE; j++)
                    {
                        switch (b[i * 4 + j])
                        {
                            case 'P':
                            case 'Q':
                            case 'K':
                            case 'N':
                            case 'R':
                            case 'B':
                                board.pieceCount++;
                                board.board[i][j].piece = b[i * 4 + j];
                                break;
                            case '-':
                                continue;
                            default:
                                Console.WriteLine("Fehler");
                                return 0;
                        }
                    }
                }
            }
            nSol = 0;
            Solve(board, root);
            return nSol;
        }
    }

    

}
