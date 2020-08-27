using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum Suit
{
	Clubs,
	Diamonds,
	Hearts,
	Spades
}

public enum Value
{
	Ace,
	Two,
	Three,
	Four,
	Five,
	Six,
	Seven,
	Eight,
	Nine,
	Ten,
	Jack,
	Queen,
	King
}

public class GameCard
{
	public GameCard(string suit, string val)
    {
		switch (suit)
		{
			case "s":
				Suit = Suit.Spades;
				break;
			case "h":
				Suit = Suit.Hearts;
				break;
			case "d":
				Suit = Suit.Diamonds;
				break;
			case "c":
				Suit = Suit.Clubs;
				break;

		}

		switch (val)
        {
			case "2":
				Value = Value.Two;
				break;
			case "3":
				Value = Value.Three;
				break;
			case "4":
				Value = Value.Four;
				break;
			case "5":
				Value = Value.Five;
				break;
			case "6":
				Value = Value.Six;
				break;
			case "7":
				Value = Value.Seven;
				break;
			case "8":
				Value = Value.Eight;
				break;
			case "9":
				Value = Value.Nine;
				break;
			case "10":
				Value = Value.Ten;
				break;
			case "J":
				Value = Value.Jack;
				break;
			case "Q":
				Value = Value.Queen;
				break;
			case "K":
				Value = Value.King;
				break;
			case "A":
				Value = Value.Ace;
				break;
		}
	}

	public Suit Suit { get; set; }
	public Value Value { get; set; }

	public int CardScore()
    {
		switch(this.Value)
        {
			case Value.Ace:
				return 13;
			case Value.King:
				return 12;
			case Value.Queen:
				return 11;
			case Value.Jack:
				return 10;
			case Value.Ten:
				return 9;
			case Value.Nine:
				return 8;
			case Value.Eight:
				return 7;
			case Value.Seven:
				return 6;
			case Value.Six:
				return 5;
			case Value.Five:
				return 4;
			case Value.Four:
				return 3;
			case Value.Three:
				return 2;
			case Value.Two:
				return 1;
		}
		return 0;
    }
}

public class Hand
{
	public IEnumerable<GameCard> Cards { get; set; }

	public int StraightScore { get; set; }

	public bool Contains(Value val)
	{
		return Cards.Where(c => c.Value == val).Any();
	}

	public bool Contains(Value val, Suit suit)
    {
		return Cards.Where(c => c.Value == val && c.Suit == suit).Any();
	}

	public int GetValueScore(Value value)
	{
		switch (value)
		{
			case Value.Ace:
				return 13;
			case Value.King:
				return 12;
			case Value.Queen:
				return 11;
			case Value.Jack:
				return 10;
			case Value.Ten:
				return 9;
			case Value.Nine:
				return 8;
			case Value.Eight:
				return 7;
			case Value.Seven:
				return 6;
			case Value.Six:
				return 5;
			case Value.Five:
				return 4;
			case Value.Four:
				return 3;
			case Value.Three:
				return 2;
			case Value.Two:
				return 1;
		}
		return 0;
	}

	public int GetHandScore()
    {
		int score = 0;
		if (IsRoyalStraightFlush)
        {
			score = 8000;
		} else if (IsStraightFlush)
        {
			score = 7500;
			score += StraightScore;
			//score += Cards.Max(c => GetValueScore(c.Value));
		}
		else if (IsFourOfAKind)
		{
			score = 7000;
			Value v = Cards.GroupBy(h => h.Value)
						.Where(g => g.Count() == 4)
						.First().Key;
			score += GetValueScore(v) * 20;
			//score += Cards.Max(c => GetValueScore(c.Value));
			score += Cards.GroupBy(h => h.Value)
					   .Where(g => g.Count() != 4).Max(c => GetValueScore(c.Key));
		}
		else if (IsFullHouse)
		{
			score = 6000;
			Value v = Cards.GroupBy(h => h.Value)
						.Where(g => g.Count() == 3)
						.First().Key;
			score += GetValueScore(v) * 20;

			var orderedPairs = Cards.GroupBy(h => h.Value).Where(g => g.Count() == 2).OrderBy(c => GetValueScore(c.Key)).ToArray();

			score += GetValueScore(orderedPairs.Last().Key);

			//score += Cards.GroupBy(h => h.Value).Where(g => g.Count() == 2).Max(c => GetValueScore(c.Key));
		}
		else if (IsFlush)
		{
			score = 5000;
			score += StraightScore;
			//score += Cards.Max(c => GetValueScore(c.Value));
		}
		else if (IsStraight)
		{
			score = 4000;
			score += StraightScore;
			//score += Cards.Max(c => GetValueScore(c.Value));
		}
		else if (IsThreeOfAKind)
		{
			score = 3000;
			Value v = Cards.GroupBy(h => h.Value)
						.Where(g => g.Count() == 3)
						.First().Key;
			score += GetValueScore(v) * 20;
			score += Cards.GroupBy(h => h.Value)
					   .Where(g => g.Count() != 3).Max(c => GetValueScore(c.Key));
			//score += Cards.Max(c => GetValueScore(c.Value));
		}
		else if (IsTwoPair)
		{
			score = 2000;
			var orderedPairs = Cards.GroupBy(h => h.Value).Where(g => g.Count() == 2).OrderBy(c => GetValueScore(c.Key)).ToArray();

			score += GetValueScore(orderedPairs.Last().Key) * 20;
			score += GetValueScore(orderedPairs[orderedPairs.Length-2].Key) * 20;
			/*Value v = Cards.GroupBy(h => h.Value)
					   .Where(g => g.Count() == 2).First().Key;
			score += GetValueScore(v) * 20;

			v = Cards.GroupBy(h => h.Value)
					   .Where(g => g.Count() == 2).Last().Key;

			score += GetValueScore(v) * 20;

			score += Cards.GroupBy(h => h.Value)
					   .Where(g => g.Count() != 2).Max(c => GetValueScore(c.Key));*/
		}
		else if (IsPair)
		{
			score = 1000;
			Value v = Cards.GroupBy(h => h.Value)
					   .Where(g => g.Count() == 2).First().Key;
			score += GetValueScore(v) * 20;
			score += Cards.Max(c => GetValueScore(c.Value));


		} else
        {
			score += Cards.Max(c => GetValueScore(c.Value));
		}


		return score;
	}

	public string GetHandName()
    {
		if (IsRoyalStraightFlush)
		{
			return "Royal Flush";
		}
		else if (IsStraightFlush)
		{
			return "Straight Flush";
		}
		else if (IsFourOfAKind)
		{
			return "Four Of A Kind";
		}
		else if (IsFullHouse)
		{
			return "Full House";
		}
		else if (IsFlush)
		{
			return "Flush";
		}
		else if (IsStraight)
		{
			return "Straight";
		}
		else if (IsThreeOfAKind)
		{
			return "Three Of A Kind";
		}
		else if (IsTwoPair)
		{
			return "Two Pair";
		}
		else if (IsPair)
		{
			return "Pair";

		}
		else
		{
			return "High Card";
		}
	}

	public bool IsPair
	{
		get
		{
			return Cards.GroupBy(h => h.Value)
					   .Where(g => g.Count() == 2)
					   .Count() == 1;
		}
	}

	public bool IsTwoPair
	{
		get
		{
			return Cards.GroupBy(h => h.Value)
						.Where(g => g.Count() == 2)
						.Count() >= 2;
		}
	}

	public bool IsThreeOfAKind
	{
		get
		{
			return Cards.GroupBy(h => h.Value)
						.Where(g => g.Count() == 3)
						.Any();
		}
	}

	public bool IsFourOfAKind
	{
		get
		{
			return Cards.GroupBy(h => h.Value)
						.Where(g => g.Count() == 4)
						.Any();
		}
	}

	public bool IsFlush
	{
		get
		{
			bool flush = Cards.GroupBy(h => h.Suit).Where(g => g.Count() >= 5).Any();
			if(flush)
            {
				Suit flushSuit = Cards.GroupBy(h => h.Suit).Where(g => g.Count() >= 5).First().Key;
				StraightScore = Cards.Where(c => c.Suit == flushSuit).Max(c => GetValueScore(c.Value));

			}
			return flush;
		}
	}

	public bool IsFullHouse
	{
		get
		{
			return (IsPair || IsTwoPair) && IsThreeOfAKind;
		}
	}

	public bool IsStraight
	{
		get
		{
			if (Contains(Value.Ace) &&
				Contains(Value.King) &&
				Contains(Value.Queen) &&
				Contains(Value.Jack) &&
				Contains(Value.Ten))
			{
				StraightScore = GetValueScore(Value.Ace);
				return true;
			}

			var ordered = Cards.OrderBy(h => h.Value).ToArray();
			
			if(Cards.Count() == 5)
            {
				var straightStart = (int)ordered.First().Value;
				for (var i = 1; i < ordered.Length; i++)
				{
					if ((int)ordered[i].Value != straightStart + i)
						return false;
				}
				StraightScore = ordered.Max(c => GetValueScore(c.Value));
				return true;
			}

			if (Cards.Count() == 6)
			{
				bool hand1 = true;
				bool hand2 = true;
				var straightStart = (int)ordered.First().Value;
				var straightStart2 = (int)ordered.ToArray<GameCard>()[1].Value;
				for (var i = 1; i < ordered.Length - 1; i++)
				{
					if ((int)ordered[i].Value != straightStart + i)
						hand1 = false;
				}
				for (var i = 2; i < ordered.Length; i++)
				{
					if ((int)ordered[i].Value != straightStart2 + i - 1)
						hand2 = false;
				}

				StraightScore = 0;

				if(hand1)
					StraightScore = UnityEngine.Mathf.Max(StraightScore, GetValueScore(ordered.ToArray<GameCard>()[4].Value));

				if (hand2)
					StraightScore = UnityEngine.Mathf.Max(StraightScore, GetValueScore(ordered.ToArray<GameCard>()[5].Value));

				return hand1 || hand2;
			}

			if (Cards.Count() == 7)
			{
				bool hand1 = true;
				bool hand2 = true;
				bool hand3 = true;
				var straightStart = (int)ordered[0].Value;
				var straightStart2 = (int)ordered[1].Value;
				var straightStart3 = (int)ordered[2].Value;
				for (var i = 1; i < ordered.Length - 2; i++)
				{
					if ((int)ordered[i].Value != straightStart + i)
						hand1 = false;
				}
				for (var i = 2; i < ordered.Length - 1; i++)
				{
					if ((int)ordered[i].Value != straightStart2 + i - 1)
						hand2 = false;
				}
				for (var i = 3; i < ordered.Length; i++)
				{
					if ((int)ordered[i].Value != straightStart3 + i - 2)
						hand3 = false;
				}

				StraightScore = 0;

				if (hand1)
					StraightScore = UnityEngine.Mathf.Max(StraightScore, GetValueScore(ordered.ToArray<GameCard>()[4].Value));

				if (hand2)
					StraightScore = UnityEngine.Mathf.Max(StraightScore, GetValueScore(ordered.ToArray<GameCard>()[5].Value));

				if (hand3)
					StraightScore = UnityEngine.Mathf.Max(StraightScore, GetValueScore(ordered.ToArray<GameCard>()[6].Value));

				return hand1 || hand2 || hand3;
			}

			return false;
		}

	}

	public bool IsStraightFlush
	{
		//get
		//{
			//return IsStraight && IsFlush;
		//}

		get
		{
			if (
				(Contains(Value.Ace, Suit.Clubs) &&
				Contains(Value.King, Suit.Clubs) &&
				Contains(Value.Queen, Suit.Clubs) &&
				Contains(Value.Jack, Suit.Clubs) &&
				Contains(Value.Ten, Suit.Clubs))
				|| (Contains(Value.Ace, Suit.Diamonds) &&
				Contains(Value.King, Suit.Diamonds) &&
				Contains(Value.Queen, Suit.Diamonds) &&
				Contains(Value.Jack, Suit.Diamonds) &&
				Contains(Value.Ten, Suit.Diamonds))
				|| (Contains(Value.Ace, Suit.Hearts) &&
				Contains(Value.King, Suit.Hearts) &&
				Contains(Value.Queen, Suit.Hearts) &&
				Contains(Value.Jack, Suit.Hearts) &&
				Contains(Value.Ten, Suit.Hearts))
				|| (Contains(Value.Ace, Suit.Spades) &&
				Contains(Value.King, Suit.Spades) &&
				Contains(Value.Queen, Suit.Spades) &&
				Contains(Value.Jack, Suit.Spades) &&
				Contains(Value.Ten, Suit.Spades))
				)
			{
				StraightScore = GetValueScore(Value.Ace);
				return true;
			}

			var ordered = Cards.OrderBy(h => h.Value).ToArray();

			if (Cards.Count() == 5)
			{
				var straightStart = (int)ordered.First().Value;
				for (var i = 1; i < ordered.Length; i++)
				{
					if ((int)ordered[i].Value != straightStart + i)
						return false;
				}
				StraightScore = ordered.Max(c => GetValueScore(c.Value));
				return Cards.GroupBy(h => h.Suit).Where(g => g.Count() >= 5).Any();
			}

			if (Cards.Count() == 6)
			{
				bool hand1 = true;
				bool hand2 = true;
				var straightStart = (int)ordered.First().Value;
				var straightStart2 = (int)ordered.ToArray<GameCard>()[1].Value;
				Suit flushStart = ordered.First().Suit;
				Suit flushStart2 = ordered.ToArray<GameCard>()[1].Suit;
				for (var i = 1; i < ordered.Length - 1; i++)
				{
					if ((int)ordered[i].Value != straightStart + i)
						hand1 = false;

					if (ordered[i].Suit != flushStart)
						hand1 = false;
				}
				for (var i = 2; i < ordered.Length; i++)
				{
					if ((int)ordered[i].Value != straightStart2 + i - 1)
						hand2 = false;

					if (ordered[i].Suit != flushStart2)
						hand2 = false;
				}

				StraightScore = 0;

				if (hand1)
					StraightScore = UnityEngine.Mathf.Max(StraightScore, GetValueScore(ordered.ToArray<GameCard>()[4].Value));

				if (hand2)
					StraightScore = UnityEngine.Mathf.Max(StraightScore, GetValueScore(ordered.ToArray<GameCard>()[5].Value));

				return hand1 || hand2;
			}

			if (Cards.Count() == 7)
			{
				bool hand1 = true;
				bool hand2 = true;
				bool hand3 = true;
				var straightStart = (int)ordered.First().Value;
				var straightStart2 = (int)ordered.ToArray<GameCard>()[1].Value;
				var straightStart3 = (int)ordered.ToArray<GameCard>()[2].Value;

				Suit flushStart = ordered.First().Suit;
				Suit flushStart2 = ordered.ToArray<GameCard>()[1].Suit;
				Suit flushStart3 = ordered.ToArray<GameCard>()[2].Suit;
				for (var i = 1; i < ordered.Length - 2; i++)
				{
					if ((int)ordered[i].Value != straightStart + i)
						hand1 = false;

					if (ordered[i].Suit != flushStart)
						hand1 = false;
				}
				for (var i = 2; i < ordered.Length - 1; i++)
				{
					if ((int)ordered[i].Value != straightStart2 + i - 1)
						hand2 = false;

					if (ordered[i].Suit != flushStart2)
						hand2 = false;
				}
				for (var i = 3; i < ordered.Length; i++)
				{
					if ((int)ordered[i].Value != straightStart3 + i - 2)
						hand3 = false;

					if (ordered[i].Suit != flushStart3)
						hand3 = false;
				}

				StraightScore = 0;

				if (hand1)
					StraightScore = UnityEngine.Mathf.Max(StraightScore, GetValueScore(ordered.ToArray<GameCard>()[4].Value));

				if (hand2)
					StraightScore = UnityEngine.Mathf.Max(StraightScore, GetValueScore(ordered.ToArray<GameCard>()[5].Value));

				if (hand3)
					StraightScore = UnityEngine.Mathf.Max(StraightScore, GetValueScore(ordered.ToArray<GameCard>()[6].Value));

				return hand1 || hand2 || hand3;
			}

			return false;
		}
	}

	public bool IsRoyalStraightFlush
	{
		get
		{
			return IsStraightFlush && Contains(Value.Ace) && Contains(Value.King);
		}
	}
}