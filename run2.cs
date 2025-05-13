using System;
using System.Collections.Generic;
using System.Linq;


class Program
{
	// Константы для символов ключей и дверей
	static readonly char[] keys_char = Enumerable.Range('a', 26).Select(i => (char)i).ToArray();
	static readonly char[] doors_char = keys_char.Select(char.ToUpper).ToArray();
    
	// Метод для чтения входных данных
	static List<List<char>> GetInput()
	{
		var data = new List<List<char>>();
		string line;
		while ((line = Console.ReadLine()) != null && line != "")
		{
			data.Add(line.ToCharArray().ToList());
		}
		return data;
	}
	
	private static (List<List<(int Node, int Mask, int Distance)>>, int) CreateGraph(List<List<char>> grid)
	{
		int rows = grid.Count;
		int cols = grid[0].Count;

		var robotPositions = new List<(int x, int y)>();
		var keyPositions = new Dictionary<char, (int key, int x, int y)>();

		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				char symbol = grid[i][j];

				if (symbol == '@')
				{
					robotPositions.Add((i, j));
				}
				else if (keys_char.Contains(symbol))
				{
					keyPositions[symbol] = (symbol, i, j);
				}
			}
		}

		int keyCount = keyPositions.Count;

		var keyIndex = keyPositions.Keys
			.OrderBy(k => k)
			.Select((k, i) => (k, i))
			.ToDictionary(pair => pair.k, pair => pair.i);

		var points = new List<(int Row, int Col)>();

		foreach (var pos in robotPositions)
		{
			points.Add((pos.x, pos.y));
		}

		foreach (char key in keyPositions.Keys.OrderBy(k => k))
		{
			var keyPos = keyPositions[key];
			points.Add((keyPos.x, keyPos.y));
		}

		var graph = new List<List<(int Node, int Mask, int Distance)>>();
		foreach (var point in points)
		{
			var edges = GetEdges(grid, point.Row, point.Col, keyIndex);
			graph.Add(edges);
		}

		return (graph, keyCount);
	}

	private static List<(int Node, int Mask, int Distance)> GetEdges(
		List<List<char>> grid,
		int startRow,
		int startCol,
		Dictionary<char, int> keyIndex)
	{
		int rows = grid.Count;
		int cols = rows > 0 ? grid[0].Count : 0;
		var edges = new List<(int Node, int Mask, int Distance)>();
		var visited = new HashSet<(int Row, int Col, int Mask)>();
		var queue = new Queue<(int Row, int Col, int Mask, int Distance)>();

		queue.Enqueue((startRow, startCol, 0, 0));
		visited.Add((startRow, startCol, 0));

		int[][] directions = new[]
		{
			new[] { -1, 0 },
			new[] { 1, 0 },
			new[] { 0, -1 },
			new[] { 0, 1 }
		};

		while (queue.Count > 0)
		{
			(int row, int col, int mask, int dist) = queue.Dequeue();

			foreach (int[] dir in directions)
			{
				int nr = row + dir[0];
				int nc = col + dir[1];

				if (nr >= 0 && nr < rows && nc >= 0 && nc < cols)
				{
					char cell = grid[nr][nc];
					if (cell == '#')
						continue;

					int newMask = mask;
					if (doors_char.Contains(cell))
					{
						int doorIdx = Array.IndexOf(doors_char, cell);
						newMask |= 1 << doorIdx;
					}

					if (keyIndex.ContainsKey(cell))
					{
						int keyPosInGraph = 4 + keyIndex[cell];
						edges.Add((keyPosInGraph, newMask, dist + 1));
					}

					if (!visited.Contains((nr, nc, newMask)))
					{
						visited.Add((nr, nc, newMask));
						queue.Enqueue((nr, nc, newMask, dist + 1));
					}
				}
			}
		}

		return edges;
	}

	private static int Solve(List<List<char>> data)
	{
		(var graph, int keyCount) = CreateGraph(data);
		if (keyCount == 0)
			return 0;

		int allKeysMask = (1 << keyCount) - 1;

		var initialState = new State(0, 1, 2, 3, 0);
		var heap = new PriorityQueue<State, int>();
		heap.Enqueue(initialState, 0);

		var minDist = new Dictionary<State, int>();
		minDist[initialState] = 0;

		while (heap.Count > 0)
		{
			heap.TryDequeue(out var currentState, out int currentDist);

			if (minDist.TryGetValue(currentState, out int dist) && dist < currentDist)
				continue;

			if (currentState.Mask == allKeysMask)
				return currentDist;

			for (int robotIdx = 0; robotIdx < 4; robotIdx++)
			{
				int[] currentPos = new[] { currentState.Pos0, currentState.Pos1, currentState.Pos2, currentState.Pos3 };
				int robotNode = currentPos[robotIdx];

				foreach (var edge in graph[robotNode])
				{
					int nextNode = edge.Node;
					int reqMask = edge.Mask;
					int addDist = edge.Distance;

					if ((currentState.Mask & reqMask) != reqMask)
						continue;

					int newMask = currentState.Mask;
					if (nextNode >= 4)
					{
						int keyBit = 1 << nextNode - 4;
						if ((currentState.Mask & keyBit) != 0)
							continue;
						newMask |= keyBit;
					}

					int[] newPos = new int[4];
					Array.Copy(currentPos, newPos, 4);
					newPos[robotIdx] = nextNode;

					var newState = new State(newPos[0], newPos[1], newPos[2], newPos[3], newMask);
					int newDist = currentDist + addDist;

					if (!minDist.ContainsKey(newState) || newDist < minDist[newState])
					{
						minDist[newState] = newDist;
						heap.Enqueue(newState, newDist);
					}
				}
			}
		}

		return -1;
	}
    
	static void Main()
	{
		var data = GetInput();
		int result = Solve(data);
        
		if (result == -1)
		{
			Console.WriteLine("No solution found");
		}
		else
		{
			Console.WriteLine(result);
		}
	}
	
	private class State : IEquatable<State>
	{

		public State(int pos0, int pos1, int pos2, int pos3, int mask)
		{
			Pos0 = pos0;
			Pos1 = pos1;
			Pos2 = pos2;
			Pos3 = pos3;
			Mask = mask;
		}
		public int Pos0 { get; }
		public int Pos1 { get; }
		public int Pos2 { get; }
		public int Pos3 { get; }
		public int Mask { get; }

		public bool Equals(State other)
		{
			return Pos0 == other.Pos0 && Pos1 == other.Pos1 &&
			       Pos2 == other.Pos2 && Pos3 == other.Pos3 && Mask == other.Mask;
		}

		public override bool Equals(object obj)
		{
			if (obj is State otherState)
				return Equals(otherState);
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Pos0, Pos1, Pos2, Pos3, Mask);
		}
	}
}
