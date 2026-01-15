# MornBeat

## 概要

音楽のBPM/拍位置から動作を制御するリズムゲーム用ライブラリ。

## 依存関係

| 種別 | 名前 |
|------|------|
| 外部パッケージ | VContainer |
| Mornライブラリ | MornLib |

## 使い方

### 楽曲情報の定義

`MornBeatMusic` ScriptableObjectで楽曲情報を定義します。

```csharp
// BPMリスト、小節情報を設定
// エディタ拡張でBPMデータの生成をサポート
```

### 再生管理

```csharp
// MornBeatPlayerで再生管理
var player = GetComponent<MornBeatPlayer>();

// BPMから自動生成するか、タイムスタンプから小節位置を計算
```

### BPM変化の定義

`MornBeatPhase` でBPM変化を定義します。
