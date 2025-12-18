# ScreenSharableWebView2
音声含め画面共有できるWebView2のサンプル。WebView2からの音声をWPF側でキャプチャし、そのままWPFアプリで再生します。

下記のQiita記事のサンプルコードとして作ったリポジトリです。
[[C#] WebView2から出る音声を画面共有で流す](https://qiita.com/HexagramNM/items/b12e977ecd5ea804f8a5)

> [!NOTE]
> NAudioの`process-audio-capture`ブランチに追加されている機能を使用する関係で、[NAudio](https://github.com/naudio/NAudio)公式のリポジトリをサブモジュールに登録しています。（`external`フォルダに入っています。）


## 環境

- OS: Windows 11 Home (24H2)
- Visual Studioのバージョン: Visual Studio Community 2022 (Ver. 17.12.3)
- .NETのバージョン: .NET 9.0

> [!NOTE]
> NAudioをローカルでビルドするために、Visual Studio Installerであらかじめ、Windows 10 SDK (10.0.18362)をインストールしてください。


## 使用しているnugetパッケージ

| パッケージ名 | バージョン | ライセンス | 備考 |
|:----:|:----:|:----:|:---- |
| [Microsoft.Web.WebView2](https://www.nuget.org/packages/Microsoft.Web.WebView2/1.0.3595.46) | 1.0.3595.46 | [BSD 3-Clause](https://www.nuget.org/packages/Microsoft.Web.WebView2/1.0.3595.46/License) ||
| [NAudio](https://github.com/naudio/NAudio) | 2.1.0-beta.1 | [MIT](https://github.com/naudio/NAudio/blob/master/license.txt) | `process-audio-capture`ブランチにあるバージョンをローカルでビルドして使用。 |
| [System.Management](https://www.nuget.org/packages/System.Management) | 10.0.0 | [MIT](https://github.com/dotnet/dotnet/blob/main/LICENSE.TXT) ||


## ビルド方法

1. SetupRepository.batを実行します。（通常の権限での実行で問題ありません。）

    - やっていることは、ローカルでNAudioをビルドし、ScreenSharableWebView2のプロジェクトで使用できるように、生成されたnugetパッケージファイルをフォルダにまとめています。（batファイルの中身を確認してから実行することをおすすめします。）詳細は以下の記事をご覧ください。
    
        - [[C#] ローカルでビルドしたNAudioをNuGetで利用する](https://qiita.com/HexagramNM/items/66902fb4e280b6523104)

    - 以下の記事も参考にしています。

        - [localのnugetパッケージを設定する](https://qiita.com/jugemjugemu/items/39c5e90c9897fda12ccc)

2. ScreenSharableWebView2.slnをVisual Studioで開き、ビルドします。

3. ビルド後`bin/(Debug or Release)/net9.0-windows/ScreenSharableWebView2.exe`ができるので、このexeファイルを実行してください。自作の配信用Webアプリ[NM_MicDisplay for Web](https://hexagramnm.coresv.com/NM_MicDisplay_Web/index.html)が開きます。そのWebアプリの音声ループバック機能を使うと、その音声がWPFのウィンドウからも流れ、画面共有で音声も共有されます。


## 免責事項

致命的なバグ等無いように注意しておりますが、このサンプルをビルド、使用したことにより問題が発生した場合も、 こちらでは責任を負いかねます。あらかじめご了承ください。
