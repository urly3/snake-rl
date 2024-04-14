using Raylib_cs;

int fps = 24;
double lastFrame = 0;
double deltaTime = 0;
double spf = 1.0f / fps;
bool paused = false;

Snake snake = new();
Grid grid = new(snake);

Raylib.InitWindow(500, 500, "snake-rl");

while (!Raylib.WindowShouldClose())
{
    input();
    update();
    render();
}

///////////

void input()
{
    if (Raylib.IsKeyPressed(KeyboardKey.Up)) snake.ChangeDirection(Direction.Up);
    else if (Raylib.IsKeyPressed(KeyboardKey.Down)) snake.ChangeDirection(Direction.Down);
    else if (Raylib.IsKeyPressed(KeyboardKey.Left)) snake.ChangeDirection(Direction.Left);
    else if (Raylib.IsKeyPressed(KeyboardKey.Right)) snake.ChangeDirection(Direction.Right);
    else if (Raylib.IsKeyPressed(KeyboardKey.P)) paused =! paused;
}

void update()
{
    deltaTime = Raylib.GetTime() - lastFrame;
    if (deltaTime < spf)
    {
        Raylib.WaitTime(spf - deltaTime);
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

    snake.UpdatePosition();
    grid.UpdateTiles();
}

void render()
{
    Raylib.BeginDrawing();

    if (!snake.Alive)
    {

        return;
    }

    if (paused)
    {
        
        return;
    }

    Raylib.EndDrawing();
}

///////////

enum Direction
{
    Up,
    Down,
    Left,
    Right,
}

enum BlockType
{
    Empty,
    Head,
    Body,
}

class Block
{
    public BlockType Type { get; set; } = BlockType.Empty;
    public Vec2d Position { get; set; } = new();

    public Block(BlockType type)
    {
        Type = type;
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
    Direction Direction { get; set; }
    public Block Head { get; set; } = new(BlockType.Head);
    public bool Alive { get; set; } = false;

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
                if (Head.Position.Y == 0)
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
                if (Head.Position.Y + 1 == Grid.Height)
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
                if (Head.Position.X == 0)
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
                if (Head.Position.X + 1 == Grid.Width)
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
}


class Grid
{
    public static int Width { get; set; }
    public static int Height { get; set; }
    public Block[] Tiles { get; set; } = new Block[Width * Height];
    private Snake snake { get; set; }

    public Grid(Snake s)
    {
        snake = s;
    }

    public void UpdateTiles()
    {
        foreach (var tile in Tiles)
        {
            tile.Type = BlockType.Empty;
        }

        for (int i = 0; i < snake.Blocks.Count; ++i)
        {
            Block block = snake.Blocks[i];
            Tiles[(block.Position.Y * Width) + block.Position.X].Type = block.Type;
        }
    }
}
