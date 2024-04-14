using Raylib_cs;

int windowWidth = 1000;
int windowHeight = 1000;
int fps = 10;
double lastFrame = 0;
double deltaTime = 0;
double spf = 1.0f / fps;
bool paused = false;
int frame = 0;

Raylib.InitWindow(windowWidth, windowHeight, "snake-rl");

Snake snake = new();
Grid grid = new(snake, windowWidth, windowHeight);


while (!Raylib.WindowShouldClose())
{
    input();
    update();
    render();
}

foreach (var texture in Block.Textures)
{
    Raylib.UnloadTexture(texture);
}

///////////

void input()
{
    if (Raylib.IsKeyPressed(KeyboardKey.Up) || Raylib.IsKeyPressed(KeyboardKey.W)) snake.ChangeDirection(Direction.Up);
    else if (Raylib.IsKeyPressed(KeyboardKey.Down) || Raylib.IsKeyPressed(KeyboardKey.S)) snake.ChangeDirection(Direction.Down);
    else if (Raylib.IsKeyPressed(KeyboardKey.Left) || Raylib.IsKeyPressed(KeyboardKey.A)) snake.ChangeDirection(Direction.Left);
    else if (Raylib.IsKeyPressed(KeyboardKey.Right) || Raylib.IsKeyPressed(KeyboardKey.D)) snake.ChangeDirection(Direction.Right);
    else if (Raylib.IsKeyPressed(KeyboardKey.P)) paused = !paused;
}

void update()
{
    deltaTime = Raylib.GetTime() - lastFrame;
    if (deltaTime < spf)
    {
        Raylib.WaitTime(spf - deltaTime);
        return;
    }

    lastFrame = Raylib.GetTime();

    if (!snake.Alive)
    {
        return;
    }

    if (paused)
    {
        return;
    }

    ++frame;

    if (frame > 10)
    {
        snake.Grow();
        frame = 0;
    }

    snake.UpdatePosition();
    grid.UpdateTiles();

    return;
}

void render()
{
    Raylib.BeginDrawing();

    if (!snake.Alive)
    {

    }

    if (paused)
    {

    }

    foreach (var tile in Grid.Tiles)
    {
        Raylib.DrawTexture(Block.Textures[(int)tile.Type], tile.Position.X * (windowWidth / Grid.Width),
            tile.Position.Y * (windowHeight / Grid.Height), Color.White);
    }

    Raylib.EndDrawing();
}

///////////

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
}

class Block
{
    public BlockType Type { get; set; }
    public Vec2d Position { get; set; } = new();
    public static Texture2D[] Textures { get; set; } = GenerateTextures(Raylib.GetScreenWidth(),
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
        Texture2D[] textures = new Texture2D[3];

        int tileWidth = windowWidth / Grid.Width;
        int tileHeight = windowHeight / Grid.Height;

        Image empty = Raylib.GenImageColor(tileWidth, tileHeight, Color.Gray);
        Image body = Raylib.GenImageColor(tileWidth, tileHeight, Color.DarkGray);
        Image head = Raylib.GenImageColor(tileWidth, tileHeight, Color.RayWhite);

        textures[0] = Raylib.LoadTextureFromImage(empty);
        textures[1] = Raylib.LoadTextureFromImage(body);
        textures[2] = Raylib.LoadTextureFromImage(head);

        Raylib.UnloadImage(empty);
        Raylib.UnloadImage(body);
        Raylib.UnloadImage(head);

        return textures;
    }
}

class Vec2d
{
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
}

class Snake
{
    public List<Block> Blocks { get; set; } = new();
    public Direction Direction { get; set; }
    public Block Head { get; set; } = new(BlockType.Head);
    public bool Alive { get; set; } = true;

    public Snake()
    {
        Head.Position.Y = Grid.Height / 2;
        Head.Position.X = Grid.Width / 2;
        Blocks.Add(Head);
    }

    public void ChangeDirection(Direction direction)
    {
        Direction = direction switch
        {
            Direction.Up => Direction == Direction.Down ? Direction.Down : Direction.Up,
            Direction.Down => Direction == Direction.Up ? Direction.Up : Direction.Down,
            Direction.Left => Direction == Direction.Right ? Direction.Right : Direction.Left,
            Direction.Right => Direction == Direction.Left ? Direction.Left : Direction.Right,
            _ => Direction
        };
    }

    public void UpdatePosition()
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

                Head.Position.Y += 1;
                return;

            case Direction.Left:
                if (Head.Position.X == 0 || Grid.Tiles[(Head.Position.Y * Grid.Width) + Head.Position.X - 1].Type == BlockType.Body)
                {
                    Alive = false;
                    return;
                }

                for (int i = Blocks.Count - 1; i > 0; --i)
                {
                    Blocks[i].Position.X = Blocks[i - 1].Position.X;
                    Blocks[i].Position.Y = Blocks[i - 1].Position.Y;
                }

                Head.Position.X -= 1;
                return;

            case Direction.Right:
                if (Head.Position.X + 1 == Grid.Width || Grid.Tiles[(Head.Position.Y * Grid.Width) + Head.Position.X + 1].Type == BlockType.Body)
                {
                    Alive = false;
                    return;
                }

                for (int i = Blocks.Count - 1; i > 0; --i)
                {
                    Blocks[i].Position.X = Blocks[i - 1].Position.X;
                    Blocks[i].Position.Y = Blocks[i - 1].Position.Y;
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
    }
}


class Grid
{
    public static int Width { get; set; } = 10;
    public static int Height { get; set; } = 10;
    public static Block[] Tiles { get; set; } = new Block[Width * Height];
    private Snake snake { get; set; }

    public Grid(Snake s, int windowWidth, int windowHeight)
    {
        for (int i = 0; i < Tiles.Count(); ++i)
        {
            Tiles[i] = new();
            Tiles[i].Position.X = i % Width;
            Tiles[i].Position.Y = (int)Math.Floor((float)i / Height);
        }
        snake = s;
    }

    public void UpdateTiles()
    {
        foreach (var tile in Tiles)
        {
            tile.Type = BlockType.Empty;
        }

        for (int i = snake.Blocks.Count - 1; i >= 0; --i)
        {
            Block block = snake.Blocks[i];
            Tiles[(block.Position.Y * Width) + block.Position.X].Type = block.Type;
        }
    }
}
