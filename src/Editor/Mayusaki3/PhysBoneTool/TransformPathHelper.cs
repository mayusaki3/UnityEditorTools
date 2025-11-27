namespace Mayusaki3.PhysBoneTool
{
    public static class TransformPathHelper
    {
        /// <summary>
        /// root から target までの Transform パスを "Root/Body/Head" のような文字列として取得
        /// </summary>
        public static string GetPath(Transform root, Transform target)
        {
            // 実装イメージ：target から root へ親を辿って、逆順に "/" 連結
        }

        /// <summary>
        /// root 配下からパスに対応する Transform を検索
        /// </summary>
        public static Transform FindByPath(Transform root, string path)
        {
            // 実装イメージ：Split('/') して逐次 Transform.Find など
        }
    }
}
