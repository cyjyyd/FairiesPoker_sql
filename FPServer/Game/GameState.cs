using Microsoft.Extensions.Logging;
using Protocol.Constant;
using Protocol.Dto.Fight;
using System.Collections.Generic;
using System.Linq;

namespace FPServer.Game
{
    /// <summary>
    /// 游戏状态枚举
    /// </summary>
    public enum GameStateEnum
    {
        WAITING,        // 等待开始
        DEALING,        // 发牌中
        GRABBING,       // 抢地主中
        PLAYING,        // 游戏进行中
        FINISHED        // 游戏结束
    }

    /// <summary>
    /// 游戏状态管理
    /// </summary>
    public class GameState
    {
        private readonly ILogger<GameState> _logger;

        // 游戏状态
        public GameStateEnum State { get; private set; } = GameStateEnum.WAITING;

        // 所有牌（54张）
        private List<CardDto> _allCards = new List<CardDto>();

        // 玩家手牌字典<userId, 手牌列表>
        private Dictionary<int, List<CardDto>> _playerCards = new Dictionary<int, List<CardDto>>();

        // 底牌（3张）
        private List<CardDto> _tableCards = new List<CardDto>();

        // 地主ID
        public int LandlordId { get; private set; } = -1;

        // 当前出牌玩家ID
        public int CurrentTurnUserId { get; private set; } = -1;

        // 上一次出牌的玩家ID（用于判断是否新一轮）
        public int LastDealUserId { get; private set; } = -1;

        // 上一次出牌信息
        public DealDto LastDeal { get; private set; }

        // 抢地主轮次
        private int _grabTurnIndex = 0;

        // 抢地主轮换顺序
        private List<int> _grabOrder = new List<int>();

        // 是否有人抢了地主
        private bool _someoneGrabbed = false;

        // 第一个抢地主的玩家（如果没人抢，由他当地主）
        private int _firstGrabUserId = -1;

        public GameState(ILogger<GameState> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 初始化游戏（发牌）
        /// </summary>
        public void InitGame(List<int> playerIds)
        {
            _logger.LogInformation("初始化游戏，玩家数量: {Count}", playerIds.Count);

            State = GameStateEnum.DEALING;
            _playerCards.Clear();
            _tableCards.Clear();
            LandlordId = -1;
            CurrentTurnUserId = -1;
            LastDealUserId = -1;
            LastDeal = null;
            _grabTurnIndex = 0;
            _someoneGrabbed = false;
            _firstGrabUserId = -1;

            // 初始化玩家手牌
            foreach (var userId in playerIds)
            {
                _playerCards[userId] = new List<CardDto>();
            }

            // 创建所有牌
            CreateAllCards();

            // 洗牌
            ShuffleCards();

            // 发牌（每人17张，留3张底牌）
            DealCards(playerIds);

            // 设置抢地主顺序
            _grabOrder = new List<int>(playerIds);

            State = GameStateEnum.GRABBING;
        }

        /// <summary>
        /// 创建所有牌
        /// </summary>
        private void CreateAllCards()
        {
            _allCards.Clear();

            // 添加大小王
            _allCards.Add(new CardDto("Joker", 0, CardWeight.SJOKER)); // 小王
            _allCards.Add(new CardDto("Joker", 0, CardWeight.LJOKER)); // 大王

            // 添加4种花色的牌（3-2，即3-15）
            string[] colors = { "Hearts", "Diamonds", "Clubs", "Spades" };
            for (int color = 0; color < 4; color++)
            {
                for (int weight = CardWeight.THREE; weight <= CardWeight.TWO; weight++)
                {
                    _allCards.Add(new CardDto(CardWeight.GetString(weight), color, weight));
                }
            }

            _logger.LogDebug("创建了 {Count} 张牌", _allCards.Count);
        }

        /// <summary>
        /// 洗牌
        /// </summary>
        private void ShuffleCards()
        {
            var random = new System.Random();
            for (int i = _allCards.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                var temp = _allCards[i];
                _allCards[i] = _allCards[j];
                _allCards[j] = temp;
            }
        }

        /// <summary>
        /// 发牌
        /// </summary>
        private void DealCards(List<int> playerIds)
        {
            int cardIndex = 0;

            // 每人发17张
            for (int i = 0; i < 17; i++)
            {
                foreach (var userId in playerIds)
                {
                    _playerCards[userId].Add(_allCards[cardIndex++]);
                }
            }

            // 剩余3张作为底牌
            for (int i = 0; i < 3; i++)
            {
                _tableCards.Add(_allCards[cardIndex++]);
            }

            _logger.LogInformation("发牌完成，底牌数量: {TableCount}", _tableCards.Count);
        }

        /// <summary>
        /// 获取玩家手牌
        /// </summary>
        public List<CardDto> GetPlayerCards(int userId)
        {
            if (_playerCards.TryGetValue(userId, out var cards))
            {
                return cards;
            }
            return new List<CardDto>();
        }

        /// <summary>
        /// 获取底牌
        /// </summary>
        public List<CardDto> GetTableCards()
        {
            return _tableCards;
        }

        /// <summary>
        /// 获取下一个抢地主的玩家ID
        /// </summary>
        public int GetNextGrabUserId()
        {
            if (_grabTurnIndex >= _grabOrder.Count)
            {
                return -1; // 抢地主结束
            }
            return _grabOrder[_grabTurnIndex];
        }

        /// <summary>
        /// 处理抢地主
        /// </summary>
        /// <param name="userId">玩家ID</param>
        /// <param name="grab">是否抢</param>
        /// <returns>返回是否抢地主成功（-1表示继续下一轮，其他表示抢地主的玩家ID）</returns>
        public int ProcessGrab(int userId, bool grab)
        {
            _logger.LogInformation("玩家 {UserId} 抢地主: {Grab}", userId, grab);

            // 记录第一个抢地主的玩家
            if (_firstGrabUserId == -1)
            {
                _firstGrabUserId = userId;
            }

            if (grab)
            {
                // 抢地主成功
                _someoneGrabbed = true;
                LandlordId = userId;

                // 给地主添加底牌
                foreach (var card in _tableCards)
                {
                    _playerCards[userId].Add(card);
                }

                // 排序手牌
                SortPlayerCards(userId);

                State = GameStateEnum.PLAYING;
                CurrentTurnUserId = userId;

                _logger.LogInformation("玩家 {UserId} 抢地主成功，获得底牌", userId);
                return userId;
            }

            // 不抢，继续下一个
            _grabTurnIndex++;

            if (_grabTurnIndex >= _grabOrder.Count)
            {
                // 所有人都不抢
                if (!_someoneGrabbed && _firstGrabUserId != -1)
                {
                    // 第一个玩家强制成为地主
                    LandlordId = _firstGrabUserId;

                    foreach (var card in _tableCards)
                    {
                        _playerCards[LandlordId].Add(card);
                    }

                    SortPlayerCards(LandlordId);

                    State = GameStateEnum.PLAYING;
                    CurrentTurnUserId = LandlordId;

                    _logger.LogInformation("没有人抢地主，玩家 {UserId} 强制成为地主", LandlordId);
                    return LandlordId;
                }

                return -1; // 异常情况
            }

            return -1; // 继续下一轮
        }

        /// <summary>
        /// 排序玩家手牌
        /// </summary>
        private void SortPlayerCards(int userId)
        {
            if (_playerCards.TryGetValue(userId, out var cards))
            {
                cards.Sort((a, b) => b.Weight.CompareTo(a.Weight));
            }
        }

        /// <summary>
        /// 处理出牌
        /// </summary>
        public bool ProcessDeal(int userId, DealDto dealDto)
        {
            if (userId != CurrentTurnUserId)
            {
                _logger.LogWarning("不是玩家 {UserId} 的回合", userId);
                return false;
            }

            // 验证手牌
            var playerCards = GetPlayerCards(userId);
            foreach (var card in dealDto.SelectCardList)
            {
                var found = playerCards.FirstOrDefault(c => c.Weight == card.Weight && c.Color == card.Color);
                if (found == null)
                {
                    _logger.LogWarning("玩家 {UserId} 没有这张牌", userId);
                    return false;
                }
            }

            // 验证牌型
            int cardType = CardType.GetCardType(dealDto.SelectCardList);
            if (cardType == CardType.NONE)
            {
                _logger.LogWarning("无效的牌型");
                return false;
            }

            // 如果需要接牌
            if (LastDeal != null && LastDealUserId != userId)
            {
                // 验证是否能管住上家
                if (!CanBeat(dealDto.SelectCardList, cardType, LastDeal.SelectCardList, LastDeal.Type))
                {
                    _logger.LogWarning("出的牌管不住上家");
                    return false;
                }
            }

            // 移除出的牌
            foreach (var card in dealDto.SelectCardList)
            {
                var toRemove = playerCards.FirstOrDefault(c => c.Weight == card.Weight && c.Color == card.Color);
                if (toRemove != null)
                {
                    playerCards.Remove(toRemove);
                }
            }

            // 更新状态
            LastDeal = dealDto;
            LastDealUserId = userId;
            dealDto.UserId = userId;
            dealDto.RemainCardList = new List<CardDto>(playerCards);

            // 设置下一个出牌的玩家
            SetNextTurnUserId(userId);

            // 检查游戏是否结束
            if (playerCards.Count == 0)
            {
                State = GameStateEnum.FINISHED;
                _logger.LogInformation("玩家 {UserId} 出完了所有牌，游戏结束", userId);
            }

            return true;
        }

        /// <summary>
        /// 处理不出
        /// </summary>
        public bool ProcessPass(int userId)
        {
            if (userId != CurrentTurnUserId)
            {
                _logger.LogWarning("不是玩家 {UserId} 的回合", userId);
                return false;
            }

            // 不能不出（如果是新一轮）
            if (LastDeal == null || LastDealUserId == userId)
            {
                _logger.LogWarning("新一轮必须出牌");
                return false;
            }

            // 设置下一个出牌的玩家
            SetNextTurnUserId(userId);

            return true;
        }

        /// <summary>
        /// 设置下一个出牌的玩家
        /// </summary>
        private void SetNextTurnUserId(int currentUserId)
        {
            var playerIds = _playerCards.Keys.ToList();
            int currentIndex = playerIds.IndexOf(currentUserId);
            int nextIndex = (currentIndex + 1) % playerIds.Count;
            CurrentTurnUserId = playerIds[nextIndex];

            // 如果轮了一圈，开始新一轮
            if (CurrentTurnUserId == LastDealUserId)
            {
                LastDeal = null; // 新一轮
            }

            _logger.LogDebug("下一个出牌的玩家: {UserId}", CurrentTurnUserId);
        }

        /// <summary>
        /// 判断是否可以管住上家
        /// </summary>
        private bool CanBeat(List<CardDto> myCards, int myType, List<CardDto> lastCards, int lastType)
        {
            // 王炸最大
            if (myType == CardType.JOKER_BOOM) return true;
            if (lastType == CardType.JOKER_BOOM) return false;

            // 炸弹大于非炸弹
            if (myType == CardType.BOOM && lastType != CardType.BOOM) return true;
            if (myType != CardType.BOOM && lastType == CardType.BOOM) return false;

            // 同类型比较
            if (myType == lastType && myCards.Count == lastCards.Count)
            {
                return CardWeight.GetWeight(myCards, myType) > CardWeight.GetWeight(lastCards, lastType);
            }

            // 炸弹之间比较
            if (myType == CardType.BOOM && lastType == CardType.BOOM)
            {
                return myCards[0].Weight > lastCards[0].Weight;
            }

            return false;
        }

        /// <summary>
        /// 检查游戏是否结束
        /// </summary>
        public bool IsGameFinished()
        {
            return State == GameStateEnum.FINISHED;
        }

        /// <summary>
        /// 获取胜利者ID列表
        /// </summary>
        public List<int> GetWinners()
        {
            var winners = new List<int>();

            if (LandlordId == -1) return winners;

            // 地主出完牌，地主赢
            if (_playerCards[LandlordId].Count == 0)
            {
                winners.Add(LandlordId);
                return winners;
            }

            // 农民都出完牌，农民赢
            foreach (var kvp in _playerCards)
            {
                if (kvp.Key != LandlordId && kvp.Value.Count == 0)
                {
                    foreach (var userId in _playerCards.Keys)
                    {
                        if (userId != LandlordId)
                        {
                            winners.Add(userId);
                        }
                    }
                    break;
                }
            }

            return winners;
        }
    }
}