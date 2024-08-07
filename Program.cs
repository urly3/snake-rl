﻿using Raylib_cs;
using System.Data.SQLite;
using RepoDb;

class Program
{
    public static int WindowWidth = 500;
    public static int WindowHeight = 500;
    public static string SqlInsert = "insert into scores(score, username) values(@score, @username)";
    public static SQLiteConnection Db = new("Data Source=scores.db");
    public static string? Username = "";

    static void Main(string[] args)
    {
        GlobalConfiguration
            .Setup()
            .UseSQLite();

        var sqlCreateTable = "CREATE TABLE scores (id integer primary key autoincrement, "
            + "score integer not null, "
            + "timestamp real not null default CURRENT_TIMESTAMP, "
            + "username text not null);";

        try
        {
            File.Open("scores.db", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }
        catch
        {
            SQLiteConnection.CreateFile("scores.db");
            Db.ExecuteNonQuery(sqlCreateTable);
        }

        Console.Write("enter a username: ");

        Username = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(Username))
        {
            throw new Exception("username cannot be empty");
        }

        Raylib.InitWindow(WindowWidth, WindowHeight, "snake-rl");

        var game = new SnakeGame();

        while (!Raylib.WindowShouldClose())
        {
            game.Play();
        }

        game.Deinit();
    }
}

static class Constants
{
    public static int fps = 60;
    public static int fpu = 10;
    public static double spf = 1.0f / fps;
    public static double spu = 1.0f / fpu;
}

enum Direction
{
    Still,
    Up,
    Down,
    Left,
    Right,
}

enum BlockType
{
    Empty,
    Body,
    Head,
    Fruit,
}

class Block
{
    public BlockType Type { get; set; }
    public Vec2d Position { get; set; } = new();
    public static Texture2D[] Textures { get; set; } =
        GenerateTextures(Raylib.GetScreenWidth(),
        Raylib.GetScreenHeight());

    public Block()
    {
        Type = BlockType.Empty;
    }

    public Block(BlockType type)
    {
        Type = type;
    }

    static Texture2D[] GenerateTextures(int windowWidth, int windowHeight)
    {
        Texture2D[] textures = new Texture2D[4];

        int tileWidth = windowWidth / Grid.Width;
        int tileHeight = windowHeight / Grid.Height;

        Image empty = Raylib.GenImageColor(tileWidth, tileHeight, Color.Gray);
        Image body = Raylib.GenImageColor(tileWidth, tileHeight, Color.DarkGray);
        Image head = Raylib.GenImageColor(tileWidth, tileHeight, Color.RayWhite);
        Image fruit = Raylib.GenImageColor(tileWidth, tileHeight, Color.Red);

        textures[0] = Raylib.LoadTextureFromImage(empty);
        textures[1] = Raylib.LoadTextureFromImage(body);
        textures[2] = Raylib.LoadTextureFromImage(head);
        textures[3] = Raylib.LoadTextureFromImage(fruit);

        Raylib.UnloadImage(empty);
        Raylib.UnloadImage(body);
        Raylib.UnloadImage(head);
        Raylib.UnloadImage(fruit);

        return textures;
    }
}

class Vec2d
{
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
}

class Grid
{
    public static int Width { get; set; } = 10;
    public static int Height { get; set; } = 10;
    public static Block[] Tiles { get; set; } = new Block[Width * Height];
    public Snake Snake { get; set; } = new();
    public Block Apple { get; set; } = new(BlockType.Fruit);
    public bool SpawnApple { get; set; } = false;

    public Grid()
    {
        for (int i = 0; i < Tiles.Count(); ++i)
        {
            Tiles[i] = new();
            Tiles[i].Position.X = i % Width;
            Tiles[i].Position.Y = (int)Math.Floor((float)i / Height);
        }
    }

    public void UpdateTiles()
    {
        foreach (var tile in Tiles)
        {
            tile.Type = BlockType.Empty;
        }

        for (int i = Snake.Blocks.Count - 1; i >= 0; --i)
        {
            Block block = Snake.Blocks[i];
            Tiles[(block.Position.Y * Width) + block.Position.X].Type = block.Type;
        }

        if (SpawnApple)
        {
            Tiles[(Apple.Position.Y * Width) + Apple.Position.X].Type = Apple.Type;
        }
    }

    public void GenerateApple()
    {
        bool unique = true;
        var random = new Random();

        int randX;
        int randY;

        do
        {
            randX = random.Next(Width);
            randY = random.Next(Height);
            unique = true;
            foreach (var block in Snake.Blocks)
            {
                if (block.Position.X == randX && block.Position.Y == randY)
                {
                    unique = false;
                    break;
                }
            }
        }
        while (!unique);

        Apple.Position.X = randX;
        Apple.Position.Y = randY;

    }

    public void Reset()
    {
        for (int i = 0; i < Tiles.Count(); ++i)
        {
            Tiles[i].Type = BlockType.Empty;
        }

        Snake.Reset();
    }
}

class Snake
{
    public List<Block> Blocks { get; set; } = new();
    public Direction Direction { get; set; } = Direction.Still;
    public Block Head { get; set; } = new(BlockType.Head);
    public bool Alive { get; set; } = true;
    public int Size { get; set; } = 1;

    public Snake()
    {
        Reset();
    }

    public void ChangeDirection(Direction direction)
    {
        Direction = direction switch
        {
            Direction.Still => Direction.Still,
            Direction.Up => Direction == Direction.Down ? Direction.Down : Direction.Up,
            Direction.Down => Direction == Direction.Up ? Direction.Up : Direction.Down,
            Direction.Left => Direction == Direction.Right ? Direction.Right : Direction.Left,
            Direction.Right => Direction == Direction.Left ? Direction.Left : Direction.Right,
            _ => Direction
        };
    }

    public void UpdatePosition(Grid grid)
    {
        switch (Direction)
        {
            case Direction.Up:
                if (Head.Position.Y == 0 || Grid.Tiles[((Head.Position.Y - 1) * Grid.Width) + Head.Position.X].Type == BlockType.Body)
                {
                    Alive = false;
                    return;
                }

                for (int i = Blocks.Count - 1; i > 0; --i)
                {
                    Blocks[i].Position.X = Blocks[i - 1].Position.X;
                    Blocks[i].Position.Y = Blocks[i - 1].Position.Y;
                }

                if (Head.Position.Y - 1 == grid.Apple.Position.Y && Head.Position.X == grid.Apple.Position.X)
                {
                    Grow();
                    grid.GenerateApple();
                }

                Head.Position.Y -= 1;

                return;

            case Direction.Down:
                if (Head.Position.Y + 1 == Grid.Height || Grid.Tiles[((Head.Position.Y + 1) * Grid.Width) + Head.Position.X].Type == BlockType.Body)
                {
                    Alive = false;
                    return;
                }

                for (int i = Blocks.Count - 1; i > 0; --i)
                {
                    Blocks[i].Position.X = Blocks[i - 1].Position.X;
                    Blocks[i].Position.Y = Blocks[i - 1].Position.Y;
                }

                if (Head.Position.Y + 1 == grid.Apple.Position.Y && Head.Position.X == grid.Apple.Position.X)
                {
                    Grow();
                    grid.GenerateApple();
                }

                Head.Position.Y += 1;

                return;

            case Direction.Left:
                if (Head.Position.X == 0 || Grid.Tiles[(Head.Position.Y * Grid.Width) + (Head.Position.X - 1)].Type == BlockType.Body)
                {
                    Alive = false;
                    return;
                }

                for (int i = Blocks.Count - 1; i > 0; --i)
                {
                    Blocks[i].Position.X = Blocks[i - 1].Position.X;
                    Blocks[i].Position.Y = Blocks[i - 1].Position.Y;
                }

                if (Head.Position.X - 1 == grid.Apple.Position.X && Head.Position.Y == grid.Apple.Position.Y)
                {
                    Grow();
                    grid.GenerateApple();
                }

                Head.Position.X -= 1;

                return;

            case Direction.Right:
                if (Head.Position.X + 1 == Grid.Width || Grid.Tiles[(Head.Position.Y * Grid.Width) + (Head.Position.X + 1)].Type == BlockType.Body)
                {
                    Alive = false;
                    return;
                }

                for (int i = Blocks.Count - 1; i > 0; --i)
                {
                    Blocks[i].Position.X = Blocks[i - 1].Position.X;
                    Blocks[i].Position.Y = Blocks[i - 1].Position.Y;
                }

                if (Head.Position.X + 1 == grid.Apple.Position.X && Head.Position.Y == grid.Apple.Position.Y)
                {
                    Grow();
                    grid.GenerateApple();
                }

                Head.Position.X += 1;

                return;
        }
    }

    public void Grow()
    {
        var block = new Block(BlockType.Body);
        block.Position.X = Head.Position.X;
        block.Position.Y = Head.Position.Y;
        Blocks.Add(block);
        Size = Blocks.Count();
    }

    public void Reset()
    {
        Blocks.Clear();
        Head.Position.Y = Grid.Height / 2;
        Head.Position.X = Grid.Width / 2;
        Blocks.Add(Head);
        Size = Blocks.Count();
        Alive = true;
    }
}

class SnakeGame
{
    static double lastUpdateFrame = 0;
    static double deltaTime = 0;
    static bool paused = false;
    static bool won = false;
    static bool goAgane = false;
    static int frame = 0;
    static bool scoreSaved = false;
    static Direction direction = Direction.Still;
    static Grid grid = new();
    int score = 0;

    public SnakeGame()
    {
    }

    public int Play()
    {
        Input();
        Update();
        Render();

        return score;
    }

    public void Deinit()
    {
        foreach (var texture in Block.Textures)
        {
            Raylib.UnloadTexture(texture);
        }
    }

    public void Input()
    {
        if (!grid.Snake.Alive)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.R)) goAgane = true;
            direction = Direction.Still;
            grid.SpawnApple = false;
            frame = 0;
            return;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Up) || Raylib.IsKeyPressed(KeyboardKey.W)) direction = Direction.Up;
        else if (Raylib.IsKeyPressed(KeyboardKey.Down) || Raylib.IsKeyPressed(KeyboardKey.S)) direction = Direction.Down;
        else if (Raylib.IsKeyPressed(KeyboardKey.Left) || Raylib.IsKeyPressed(KeyboardKey.A)) direction = Direction.Left;
        else if (Raylib.IsKeyPressed(KeyboardKey.Right) || Raylib.IsKeyPressed(KeyboardKey.D)) direction = Direction.Right;
        else if (Raylib.IsKeyPressed(KeyboardKey.P)) paused = !paused;
    }

    public void Update()
    {
        deltaTime = Raylib.GetTime() - lastUpdateFrame;

        if (deltaTime < Constants.spu)
        {
            Raylib.WaitTime(Constants.spf);
            return;
        }

        ++frame;

        lastUpdateFrame = Raylib.GetTime();

        if (goAgane)
        {
            grid.Reset();
            goAgane = false;
            scoreSaved = false;
            return;
        }

        if (grid.Snake.Size == Grid.Width * Grid.Height)
        {
            won = true;
        }

        if (!grid.Snake.Alive)
        {
            if (!scoreSaved)
            {
                _ = Program.Db.ExecuteNonQuery(Program.SqlInsert, new { score = grid.Snake.Size, username = Program.Username });
                scoreSaved = true;
            }

            return;
        }

        if (paused || won)
        {
            return;
        }

        if (!grid.SpawnApple && frame > (1.0f / Constants.spu) * 2)
        {
            grid.GenerateApple();
            grid.SpawnApple = true;
        }

        grid.Snake.ChangeDirection(direction);
        grid.Snake.UpdatePosition(grid);
        grid.UpdateTiles();

        return;
    }

    public void Render()
    {
        Raylib.BeginDrawing();

        if (paused || won) { }

        foreach (var tile in Grid.Tiles)
        {
            Raylib.DrawTexture(Block.Textures[(int)tile.Type], tile.Position.X * (Program.WindowWidth / Grid.Width),
                tile.Position.Y * (Program.WindowHeight / Grid.Height), Color.White);
        }

        if (!grid.Snake.Alive)
        {
            Raylib.DrawText("you died. press 'r' to restart.", 20, 10, 30, Color.Maroon);

            int y = 100;
            Raylib.DrawText("local high scores", (Program.WindowWidth / 2) - 140, y, 30, Color.Black);
            using (var reader = Program.Db.ExecuteReader("select username,score from scores order by score desc limit 5;"))
            {
                var i = 0;

                while (reader.Read())
                {
                    var colour = i++ switch
                    {
                        0 => Color.Gold,
                        1 => Color.RayWhite,
                        2 => Color.Brown,
                        _ => Color.Black
                    };

                    y += 40;

                    var username = reader.GetString(0);
                    var score = reader.GetInt32(1);
                    Raylib.DrawText(username, (Program.WindowWidth / 2) - 140, y, 30, colour);
                    Raylib.DrawText(score.ToString(), (Program.WindowWidth / 2) + 80, y, 30, colour);
                }
            }
        }
        else
        {
            Raylib.DrawText(grid.Snake.Size.ToString(), 10, 10, 21, Color.Black);
        }

        Raylib.EndDrawing();
    }
}
