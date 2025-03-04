using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

using static UIManager;
using static PDDLHelper;
using static FastDownwardIntegration;

public class GameManager : MonoBehaviour
{
    public Tilemap groundTilemap;
    public Tilemap horizontalWallsTilemap;
    public Tilemap verticalWallsTilemap;
    public Tilemap onGroundTilemapInspector;

    public static Tilemap onGroundTilemap;

    public int rows;
    public int columns; 

    public class Tile
    {
        // CANT HAVE MORE THAN 26 COLUMNS

        public bool blocked;
        public int row;
        public int col;
        public Vector3Int position;

        public Tile(bool blocked, int row, int col, Vector3Int position)
        {
            this.blocked = blocked;
            this.row = row;
            this.col = col;
            this.position = position;
        }

        public virtual void MakeAction(UIManager uIManager) { }
    }
    public class WallTrigger : Tile
    {
        public Wall wall;
        public bool activateWall;
        int id;

        public WallTrigger(bool blocked, int row, int col, Vector3Int position, Wall wall, bool activateWall, int id) : base(blocked, row, col, position)
        {
            this.wall = wall;
            this.activateWall = activateWall;
            this.id = id;
        }

        public override void MakeAction(UIManager uIManager)
        {
            wall.SetActive(activateWall, uIManager);
        }
    }
    public class Wall
    {
        public Tile tile1;
        public Tile tile2;
        public Vector3Int position;
        public TileBase tileBase;
        public bool horizontal;
        private bool active;
        public Wall(Tile tile1, Tile tile2, Vector3Int position, bool horizontal, TileBase tileBase)
        {
            this.tile1 = tile1;
            this.tile2 = tile2;
            this.position = position;
            this.horizontal = horizontal;
            this.tileBase = tileBase;
        }
        public void SetActive(bool active, UIManager uIManager)
        {
            this.active = active;
            uIManager.WallSetActive(this, active);
        }
    }

    public class Enemy
    {
        public Tile tilePosition;
        public TileBase tileBase;
        public string PDDLNotation;
        public Enemy(Tile tilePosition, TileBase tileBase)
        {
            this.tilePosition = tilePosition;
            this.tileBase = tileBase;
        }
    }

    public class Enemies
    {
        public List<Enemy> enemies;
        public void MakeMove()
        {

        }
    }

    public abstract class Action
    {
        public abstract void makeAction(UIManager uIManager);
    }
    public class MoveAction : Action
    {
        Enemy enemy;
        Tile toTile;
        public override void makeAction(UIManager uIManager)
        {
            MoveEnemyToTile(enemy, toTile, onGroundTilemap);
            enemy.tilePosition = toTile;
            toTile.MakeAction(uIManager);
        }
        public MoveAction(Enemy enemy, Tile toTile)
        {
            this.enemy = enemy;
            this.toTile = toTile;
        }
    }

    public class Board
    {
        public int rows;
        public int columns;
        public Tile[,] tiles;
        public List<Enemy> enemies;
        public List<Wall> walls;
        public List<WallTrigger> wallTriggers;
        public List<WallTrigger> reverseWallTriggers;
    }

    Board board;

    /*
    private void InitBoards()
    {
        basic_board = new Board();
        basic_board.columns = 6;
        basic_board.rows = 6;
        basic_board.startLocation = new Tile(5, 3);
        basic_board.blocked = new List<Tile>()
        {
            new Tile(4, 3),
            new Tile(4, 4),
            new Tile(4, 2),
            new Tile(4, 1),
        };
        basic_board.walls = new List<Wall>()
        {
            new Wall(1, 1, 2, 1),
            new Wall(1, 1, 1, 2),
        };
        basic_board.wallTriggers = new List<WallTrigger>()
        {
            new WallTrigger(new Tile(5, 5), new Wall(4, 5, 3, 5))
        };


        complex_board = new Board();
        complex_board.columns = 8;
        complex_board.rows = 8;
        complex_board.startLocation = new Tile(7, 4);
        complex_board.blocked = new List<Tile>()
        {
            new Tile(1, 0),
            new Tile(1, 1),
            new Tile(1, 2),
            new Tile(1, 3),
            new Tile(2, 3),
            new Tile(3, 1),
            new Tile(3, 6),
            new Tile(3, 7),
            new Tile(4, 1),
            new Tile(4, 4),
            new Tile(5, 1),
            new Tile(5, 6),
            new Tile(6, 1),
            new Tile(6, 2),
            new Tile(6, 4),
            new Tile(6, 6),
        };
        complex_board.wallTriggers = new List<WallTrigger>()
        {
            // A
            new WallTrigger(new Tile(6, 3), new Wall(3, 3, 4, 3)),
            new WallTrigger(new Tile(6, 3), new Wall(1, 5, 2, 5)),
            // B
            new WallTrigger(new Tile(6, 5), new Wall(3, 5, 4, 5)),
            new WallTrigger(new Tile(6, 5), new Wall(1, 4, 2, 4)),
            // C
            new WallTrigger(new Tile(4, 6), new Wall(2, 5, 3, 5)),
            new WallTrigger(new Tile(4, 6), new Wall(1, 4, 2, 4)),
        };
    }
    */

    List<Action> actions;
    int actionIndex;

    UIManager uiManager;

    void Start()
    {
        onGroundTilemap = onGroundTilemapInspector;

        string problemName = "tilemap_generated";
        string domainName = "tilemap_generated_domain";
        string planName = "tilemap_generated_plan";

        uiManager = new UIManager(
            groundTilemap,
            horizontalWallsTilemap,
            verticalWallsTilemap,
            onGroundTilemap,
            rows,
            columns
         );
        board = uiManager.GetBoardFromTilemaps();

        /*
        Debug.Log("Wall triggers count: " + board.wallTriggers.Count);
        WallTrigger wallTrigger = board.wallTriggers[0];
        Debug.Log("Trigger: row " + wallTrigger.row + " col " + wallTrigger.col);
        Debug.Log("Wall: row " + wallTrigger.wall.tile1.row + " col " + wallTrigger.wall.tile1.col);
        */



        CreatePDDLProblemFile(problemName, board, domainName);
        RunFastDownward(problemName, domainName, planName);

        actions = GetActionsFromPlan(planName, board);
        actionIndex = 0;
    }

    public void MakeNextAction()
    {
        if (actionIndex < actions.Count)
        {
            actions[actionIndex].makeAction(uiManager);
        }
        actionIndex++;
    }
}
