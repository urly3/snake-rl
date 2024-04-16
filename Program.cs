using Raylib_cs;

int windowWidth = 400;
int windowHeight = 400;
int fps = 60;
double lastUpdateFrame = 0;
double deltaTime = 0;
double spf = 1.0f / fps;
int fpu = 10;
double spu = 1.0f / fpu;
bool paused = false;
bool won = false;
int frame = 0;
Direction direction = Direction.Still;

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
    if (!snake.Alive) return;
    if (Raylib.IsKeyPressed(KeyboardKey.Up) || Raylib.IsKeyPressed(KeyboardKey.W)) direction = Direction.Up;
    else if (Raylib.IsKeyPressed(KeyboardKey.Down) || Raylib.IsKeyPressed(KeyboardKey.S)) direction = Direction.Down;
    else if (Raylib.IsKeyPressed(KeyboardKey.Left) || Raylib.IsKeyPressed(KeyboardKey.A)) direction = Direction.Left;
    else if (Raylib.IsKeyPressed(KeyboardKey.Right) || Raylib.IsKeyPressed(KeyboardKey.D)) direction = Direction.Right;
    else if (Raylib.IsKeyPressed(KeyboardKey.P)) paused = !paused;
}

void update()
{
    deltaTime = Raylib.GetTime() - lastUpdateFrame;

    if (deltaTime < spu)
    {
        Raylib.WaitTime(spf);

        return;
    }

    ++frame;

    lastUpdateFrame = Raylib.GetTime();

    if (snake.Size == Grid.Width * Grid.Height)
    {
        won = true;
    }


    if (!snake.Alive)
    {
        return;
    }

    if (paused)
    {
        return;
    }

    if (won)
    {
        return;
    }

    if (!grid.SpawnApple && frame > (1.0f / spu) * 2)
    {
        grid.GenerateApple(snake);
        grid.SpawnApple = true;
    }

    snake.ChangeDirection(direction);
    snake.UpdatePosition(grid);
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

    if (won)
    {

    }

    foreach (var tile in Grid.Tiles)
    {
        Raylib.DrawTexture(Block.Textures[(int)tile.Type], tile.Position.X * (windowWidth / Grid.Width),
            tile.Position.Y * (windowHeight / Grid.Height), Color.White);
    }

    Raylib.DrawText("size: " + snake.Size.ToString(), 10, 10, 21, Color.Black);

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
    Fruit,
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

class Snake
{
    public List<Block> Blocks { get; set; } = new();
    public Direction Direction { get; set; }
    public Block Head { get; set; } = new(BlockType.Head);
    public bool Alive { get; set; } = true;
    public int Size { get; set; } = 1;

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
                    grid.GenerateApple(this);
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
                    grid.GenerateApple(this);
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
                    grid.GenerateApple(this);
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
                    grid.GenerateApple(this);
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
        ++Size;
    }
}


class Grid
{
    public static int Width { get; set; } = 10;
    public static int Height { get; set; } = 10;
    public static Block[] Tiles { get; set; } = new Block[Width * Height];
    private Snake snake { get; set; }
    public Block Apple { get; set; } = new();
    public bool SpawnApple { get; set; } = false;

    public Grid(Snake s, int windowWidth, int windowHeight)
    {
        for (int i = 0; i < Tiles.Count(); ++i)
        {
            Tiles[i] = new();
            Tiles[i].Position.X = i % Width;
            Tiles[i].Position.Y = (int)Math.Floor((float)i / Height);
        }

        Apple.Type = BlockType.Fruit;

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

        if (SpawnApple)
        {
            Tiles[(Apple.Position.Y * Width) + Apple.Position.X].Type = Apple.Type;
        }
    }

    public void GenerateApple(Snake snake)
    {
        while (true)
        {
            var random = new Random();
            int randX = random.Next(Width);
            int randY = random.Next(Height);

            bool unique = true;

            foreach (var block in snake.Blocks)
            {
                if (block.Position.X == randX && block.Position.Y == randY)
                {
                    unique = false;
                    break;
                }
            }

            if (!unique)
            {
                continue;
            }

            Apple.Position.X = randX;
            Apple.Position.Y = randY;

            break;
        }
    }
}
