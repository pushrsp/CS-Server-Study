#!/bin/bash

dotnet ../PacketGenerator/bin/PacketGenerator.dll /Users/pushrsp/Desktop/lecture/game/Review/Server6/PacketGenerator/PDL.xml

cp ./GenPackets.cs ../Dummy/Packet/GenPackets.cs
cp ./GenPackets.cs ../Server/Packet/GenPackets.cs

cp ./ClientPacketManager.cs ../Dummy/Packet/
cp ./ServerPacketManager.cs ../Server/Packet/

rm -rf ./GenPackets.cs
rm -rf ./ClientPacketManager.cs
rm -rf ./ServerPacketManager.cs
