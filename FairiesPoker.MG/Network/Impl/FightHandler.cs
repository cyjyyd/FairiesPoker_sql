using Protocol.Code;
using Protocol.Constant;
using Protocol.Dto.Fight;
using System.Collections;
using System.Collections.Generic;

namespace FairiesPoker.MG.Network.Impl
{

public class FightHandler : HandlerBase
{
    public override void OnReceive(int subCode, object value)
    {
        switch (subCode)
        {
            case FightCode.GET_CARD_SRES:
                getCards(value as List<CardDto>);
                break;
            case FightCode.TURN_GRAB_BRO:
                turnGrabBro((int)value);
                break;
            case FightCode.GRAB_LANDLORD_BRO:
                grabLandlordBro(value as GrabDto);
                break;
            case FightCode.TURN_DEAL_BRO:
                turnDealBro((int)value);
                break;
            case FightCode.DEAL_BRO:
                dealBro(value as DealDto);
                break;
            case FightCode.DEAL_SRES:
                dealResponse((int)value);
                break;
            case FightCode.PASS_SRES:
                passResponse((int)value);
                break;
            case FightCode.OVER_BRO:
                overBro(value as OverDto);
                break;
            case FightCode.REFRESH_MULTIPLE:
                changeMultiple((int)value);
                break;
            default:
                break;
        }
    }

    private void changeMultiple(int value)
    {
        Models.TriggerMultipleChange(value);
    }

    /// <summary>
    /// 结束广播
    /// </summary>
    /// <param name="dto"></param>
    private void overBro(OverDto dto)
    {
        Models.TriggerGameOver(dto);
    }

    /// <summary>
    /// 出牌响应
    /// </summary>
    /// <param name=""></param>
    private void dealResponse(int result)
    {
        Models.TriggerDealResponse(result);
    }

    private void passResponse(int result)
    {
        Models.TriggerPassResponse(result);
    }

    /// <summary>
    /// 同步出牌
    /// </summary>
    /// <param name="dto"></param>
    private void dealBro(DealDto dto)
    {
        Models.TriggerDealBroadcast(dto);
    }

    /// <summary>
    /// 播放出牌音效
    /// </summary>
    private void playDealAudio(int cardType, int weight)
    {
        string audioName = "Fight/";
        //完善（播放出牌音效）
        switch (cardType)
        {
            case CardType.SINGLE:
                audioName += "Woman_" + weight;
                break;
            case CardType.DOUBLE:
                audioName += "Woman_dui" + weight / 2;
                break;
            case CardType.STRAIGHT:
                audioName += "Woman_shunzi";
                break;
            case CardType.DOUBLE_STRAIGHT:
                audioName += "Woman_liandui";
                break;
            case CardType.TRIPLE_STRAIGHT:
                audioName += "Woman_feiji";
                break;
            case CardType.THREE:
                audioName += "Woman_tuple" + weight / 3;
                break;
            case CardType.THREE_ONE:
                audioName += "Woman_sandaiyi";
                break;
            case CardType.THREE_TWO:
                audioName += "Woman_sandaiyidui";
                break;
            case CardType.BOOM:
                audioName += "Woman_zhadan";
                break;
            case CardType.JOKER_BOOM:
                audioName += "Woman_wangzha";
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 转换出牌
    /// </summary>
    /// <param name="userId">出牌者id</param>
    private void turnDealBro(int userId)
    {
        Models.TriggerTurnDeal(userId);
    }

    /// <summary>
    /// 抢地主成功的处理
    /// </summary>
    private void grabLandlordBro(GrabDto dto)
    {
        Models.TriggerGrabLandlord(dto);
    }

    /// <summary>
    /// 是否是第一个玩家抢地主 而不是 因为别的玩家不叫地主而转换的
    /// </summary>
    private bool isFirst = true;

    /// <summary>
    /// 转换抢地主
    /// </summary>
    /// <param name="userId"></param>
    private void turnGrabBro(int userId)
    {
        Models.TriggerTurnGrab(userId);
    }

    /// <summary>
    /// 获取到卡牌的处理
    /// </summary>
    /// <param name="cardList"></param>
    private void getCards(List<CardDto> cardList)
    {
        Models.TriggerGetCards(cardList);
    }
}}
