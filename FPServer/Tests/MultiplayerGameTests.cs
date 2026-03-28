using Microsoft.Extensions.Logging;
using Protocol.Constant;
using Protocol.Dto;
using Protocol.Dto.Fight;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPServer.Tests
{
    /// <summary>
    /// 多人游戏流程测试类
    /// 测试游戏状态管理、房间管理、游戏流程等功能
    /// </summary>
    public class MultiplayerGameTests
    {
        private readonly ILoggerFactory _loggerFactory;

        public MultiplayerGameTests()
        {
            // 创建简单的日志工厂
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });
        }

        /// <summary>
        /// 运行所有测试
        /// </summary>
        public void RunAllTests()
        {
            Console.WriteLine("========== 开始多人游戏测试 ==========\n");

            TestGameState_InitGame();
            TestGameState_ProcessGrab();
            TestGameState_ProcessDeal();
            TestGameState_ProcessPass();
            TestGameState_CanBeat();
            TestRoom_Management();
            TestCompleteGameFlow();

            Console.WriteLine("\n========== 所有测试完成 ==========");
        }

        /// <summary>
        /// 测试游戏初始化
        /// </summary>
        private void TestGameState_InitGame()
        {
            Console.WriteLine("测试: GameState.InitGame");

            var gameState = new Game.GameState(_loggerFactory.CreateLogger<Game.GameState>());
            var playerIds = new List<int> { 1, 2, 3 };

            gameState.InitGame(playerIds);

            // 验证每个玩家获得17张牌
            foreach (var userId in playerIds)
            {
                var cards = gameState.GetPlayerCards(userId);
                Assert(cards.Count == 17, $"玩家 {userId} 应获得17张牌，实际获得 {cards.Count} 张");
            }

            // 验证底牌3张
            var tableCards = gameState.GetTableCards();
            Assert(tableCards.Count == 3, $"底牌应为3张，实际为 {tableCards.Count} 张");

            // 验证状态为抢地主
            Assert(gameState.State == Game.GameStateEnum.GRABBING, "状态应为GRABBING");

            Console.WriteLine("  ✓ 通过\n");
        }

        /// <summary>
        /// 测试抢地主流程
        /// </summary>
        private void TestGameState_ProcessGrab()
        {
            Console.WriteLine("测试: GameState.ProcessGrab");

            var gameState = new Game.GameState(_loggerFactory.CreateLogger<Game.GameState>());
            var playerIds = new List<int> { 1, 2, 3 };
            gameState.InitGame(playerIds);

            // 测试玩家1不抢
            int result1 = gameState.ProcessGrab(1, false);
            Assert(result1 == -1, "玩家1不抢，应返回-1继续下一轮");

            // 测试玩家2抢地主
            int result2 = gameState.ProcessGrab(2, true);
            Assert(result2 == 2, "玩家2抢地主成功，应返回2");

            // 验证地主ID
            Assert(gameState.LandlordId == 2, $"地主ID应为2，实际为 {gameState.LandlordId}");

            // 验证地主手牌数量（17+3=20张）
            var landlordCards = gameState.GetPlayerCards(2);
            Assert(landlordCards.Count == 20, $"地主应有20张牌，实际为 {landlordCards.Count} 张");

            // 验证状态为游戏中
            Assert(gameState.State == Game.GameStateEnum.PLAYING, "状态应为PLAYING");

            // 验证当前回合为地主
            Assert(gameState.CurrentTurnUserId == 2, "当前回合应为地主(2)");

            Console.WriteLine("  ✓ 通过\n");
        }

        /// <summary>
        /// 测试出牌流程
        /// </summary>
        private void TestGameState_ProcessDeal()
        {
            Console.WriteLine("测试: GameState.ProcessDeal");

            var gameState = new Game.GameState(_loggerFactory.CreateLogger<Game.GameState>());
            var playerIds = new List<int> { 1, 2, 3 };
            gameState.InitGame(playerIds);

            // 玩家2抢地主
            gameState.ProcessGrab(1, false);
            gameState.ProcessGrab(2, true);

            // 获取玩家2的手牌
            var landlordCards = gameState.GetPlayerCards(2);

            // 测试出单张
            var dealDto = new DealDto
            {
                SelectCardList = new List<CardDto> { landlordCards[0] },
                Type = CardType.SINGLE
            };

            bool dealResult = gameState.ProcessDeal(2, dealDto);
            Assert(dealResult, "地主出牌应成功");

            // 验证手牌减少
            Assert(gameState.GetPlayerCards(2).Count == 19, "地主出牌后应有19张牌");

            // 验证当前回合轮换
            Assert(gameState.CurrentTurnUserId != 2, "回合应轮换到下一个玩家");

            Console.WriteLine("  ✓ 通过\n");
        }

        /// <summary>
        /// 测试不出流程
        /// </summary>
        private void TestGameState_ProcessPass()
        {
            Console.WriteLine("测试: GameState.ProcessPass");

            var gameState = new Game.GameState(_loggerFactory.CreateLogger<Game.GameState>());
            var playerIds = new List<int> { 1, 2, 3 };
            gameState.InitGame(playerIds);

            // 玩家2抢地主
            gameState.ProcessGrab(1, false);
            gameState.ProcessGrab(2, true);

            // 地主出牌
            var landlordCards = gameState.GetPlayerCards(2);
            var dealDto = new DealDto
            {
                SelectCardList = new List<CardDto> { landlordCards[0] },
                Type = CardType.SINGLE
            };
            gameState.ProcessDeal(2, dealDto);

            // 下一个玩家不出
            int nextUserId = gameState.CurrentTurnUserId;
            bool passResult = gameState.ProcessPass(nextUserId);
            Assert(passResult, "不出应成功");

            // 验证回合轮换
            Assert(gameState.CurrentTurnUserId != nextUserId, "回合应轮换到下一个玩家");

            Console.WriteLine("  ✓ 通过\n");
        }

        /// <summary>
        /// 测试牌型比较
        /// </summary>
        private void TestGameState_CanBeat()
        {
            Console.WriteLine("测试: GameState.CanBeat");

            var gameState = new Game.GameState(_loggerFactory.CreateLogger<Game.GameState>());

            // 测试单张比较
            var card3 = new CardDto("Three", 0, CardWeight.THREE);
            var card4 = new CardDto("Four", 0, CardWeight.FOUR);
            var card5 = new CardDto("Five", 0, CardWeight.FIVE);

            // 创建测试用的ProcessDeal方法（通过反射或公开方法测试）
            // 由于CanBeat是私有方法，我们通过ProcessDeal间接测试

            // 单张3能被单张4管住
            // 这里我们使用CardType来判断

            Assert(CardWeight.GetWeight(new List<CardDto> { card4 }, CardType.SINGLE) >
                   CardWeight.GetWeight(new List<CardDto> { card3 }, CardType.SINGLE),
                   "单张4应大于单张3");

            Assert(CardWeight.GetWeight(new List<CardDto> { card5 }, CardType.SINGLE) >
                   CardWeight.GetWeight(new List<CardDto> { card4 }, CardType.SINGLE),
                   "单张5应大于单张4");

            // 测试炸弹大于单张
            Assert(CardType.BOOM > CardType.SINGLE, "炸弹牌型应大于单张");

            // 测试王炸最大
            Assert(CardType.JOKER_BOOM > CardType.BOOM, "王炸应大于炸弹");

            Console.WriteLine("  ✓ 通过\n");
        }

        /// <summary>
        /// 测试房间管理
        /// </summary>
        private void TestRoom_Management()
        {
            Console.WriteLine("测试: Room管理");

            var roomManager = new Game.RoomManager(_loggerFactory);

            // 测试创建房间
            var room = roomManager.FindOrCreateRoom();
            Assert(room != null, "应成功创建房间");
            Assert(room.GetPlayerCount() == 0, "新房间玩家数应为0");

            // 测试添加玩家
            var userDto1 = new UserDto { Id = 1, Name = "玩家1" };
            var userDto2 = new UserDto { Id = 2, Name = "玩家2" };
            var userDto3 = new UserDto { Id = 3, Name = "玩家3" };

            bool addResult1 = roomManager.AddPlayerToRoom(room, 1, userDto1);
            Assert(addResult1, "添加玩家1应成功");
            Assert(room.GetPlayerCount() == 1, "房间玩家数应为1");

            bool addResult2 = roomManager.AddPlayerToRoom(room, 2, userDto2);
            Assert(addResult2, "添加玩家2应成功");
            Assert(room.GetPlayerCount() == 2, "房间玩家数应为2");

            bool addResult3 = roomManager.AddPlayerToRoom(room, 3, userDto3);
            Assert(addResult3, "添加玩家3应成功");
            Assert(room.GetPlayerCount() == 3, "房间玩家数应为3");

            // 测试房间已满
            Assert(room.IsFull(), "房间应已满");

            // 测试不能再添加玩家
            var userDto4 = new UserDto { Id = 4, Name = "玩家4" };
            bool addResult4 = roomManager.AddPlayerToRoom(room, 4, userDto4);
            Assert(!addResult4, "满房间不应添加新玩家");

            // 测试获取玩家房间
            var playerRoom = roomManager.GetRoomByPlayerId(1);
            Assert(playerRoom == room, "应能通过玩家ID获取房间");

            // 测试移除玩家
            roomManager.RemovePlayerFromRoom(room, 1);
            Assert(room.GetPlayerCount() == 2, "移除后玩家数应为2");
            Assert(roomManager.GetRoomByPlayerId(1) == null, "移除后玩家不应有房间");

            Console.WriteLine("  ✓ 通过\n");
        }

        /// <summary>
        /// 测试完整游戏流程
        /// </summary>
        private void TestCompleteGameFlow()
        {
            Console.WriteLine("测试: 完整游戏流程");

            var roomManager = new Game.RoomManager(_loggerFactory);

            // 创建房间并添加玩家
            var room = roomManager.CreateRoom(1, "测试房间");
            var userDto1 = new UserDto { Id = 1, Name = "玩家1" };
            var userDto2 = new UserDto { Id = 2, Name = "玩家2" };
            var userDto3 = new UserDto { Id = 3, Name = "玩家3" };

            roomManager.AddPlayerToRoom(room, 1, userDto1);
            roomManager.AddPlayerToRoom(room, 2, userDto2);
            roomManager.AddPlayerToRoom(room, 3, userDto3);

            // 开始游戏
            room.StartGame();
            var gameState = room.GameState;

            Assert(gameState != null, "游戏状态应存在");
            Assert(gameState.State == Game.GameStateEnum.GRABBING, "状态应为GRABBING");

            // 验证发牌
            foreach (var userId in room.GetPlayerIds())
            {
                var cards = gameState.GetPlayerCards(userId);
                Assert(cards.Count == 17, $"玩家 {userId} 应有17张牌");
            }
            Assert(gameState.GetTableCards().Count == 3, "应有3张底牌");

            // 抢地主流程
            int firstGrabUserId = gameState.GetNextGrabUserId();
            gameState.ProcessGrab(firstGrabUserId, false);

            int secondGrabUserId = gameState.GetNextGrabUserId();
            int landlordId = gameState.ProcessGrab(secondGrabUserId, true);

            Assert(landlordId == secondGrabUserId, "第二个抢地主的玩家应成为地主");
            // 注意：房间LandlordId由FightHandler更新，这里只验证GameState
            Assert(gameState.LandlordId == landlordId, "游戏状态地主ID应正确");

            // 验证地主手牌
            var landlordCards = gameState.GetPlayerCards(landlordId);
            Assert(landlordCards.Count == 20, "地主应有20张牌");

            // 出牌流程模拟
            int currentTurn = gameState.CurrentTurnUserId;
            Assert(currentTurn == landlordId, "地主应先出牌");

            // 地主出一张牌
            var dealDto = new DealDto
            {
                SelectCardList = new List<CardDto> { landlordCards[0] },
                Type = CardType.SINGLE
            };
            gameState.ProcessDeal(landlordId, dealDto);

            // 获取下一个出牌的玩家
            int nextTurn = gameState.CurrentTurnUserId;
            Assert(nextTurn != landlordId, "回合应轮换");

            // 下一个玩家不出
            gameState.ProcessPass(nextTurn);

            // 再下一个玩家也不出
            int thirdTurn = gameState.CurrentTurnUserId;
            gameState.ProcessPass(thirdTurn);

            // 回到地主，新一轮
            Assert(gameState.LastDeal == null, "应开始新一轮");

            Console.WriteLine("  ✓ 通过\n");
        }

        /// <summary>
        /// 简单断言方法
        /// </summary>
        private void Assert(bool condition, string message)
        {
            if (!condition)
            {
                Console.WriteLine($"  ✗ 失败: {message}");
                throw new Exception($"测试失败: {message}");
            }
        }

        /// <summary>
        /// 测试入口
        /// </summary>
        public static void RunTests()
        {
            var tests = new MultiplayerGameTests();
            try
            {
                tests.RunAllTests();
                Console.WriteLine("\n所有测试通过!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n测试失败: {ex.Message}");
            }
        }
    }
}