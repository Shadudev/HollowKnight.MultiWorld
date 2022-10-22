﻿namespace MultiWorldLib.Messaging
{
    public enum MWMessageType
    {
        InvalidMessage=0,
        SharedCore=1,
        ConnectMessage,
        DisconnectMessage,
        JoinMessage,
        JoinConfirmMessage,
        LeaveMessage,
        DataReceiveMessage,
        DataReceiveConfirmMessage,
        DataSendMessage,
        DataSendConfirmMessage,
        ReadyConfirmMessage,
        ReadyDenyMessage,
        PingMessage,
        ReadyMessage,
        ResultMessage,
        SaveMessage,
        SetupMessage,
        RandoGeneratedMessage,
        UnreadyMessage,
        InitiateGameMessage,
        RequestRandoMessage,
        AnnounceCharmNotchCostsMessage,
        RequestCharmNotchCostsMessage,
        ConfirmCharmNotchCostsReceivedMessage,
        DatasSendMessage,
        DatasSendConfirmMessage,
        InitiateSyncGameMessage,
        ApplySettingsMessage,
        RequestSettingsMessage,
        ISReadyMessage,
        DatasReceiveMessage,
        DatasReceiveConfirmMessage,
    }
}
