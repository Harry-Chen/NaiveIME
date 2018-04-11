# NaiveIME

# 编译

理论上本程序能够在任何支持 `.NET Standard 2.0` 的平台上编译运行，在下列平台经过测试：

* Windows + Visual Studio 2017 (.NET Framework 4.6.1)
* Debian Linux + Mono 5.10.1

编译方法为在项目目录下运行： 

```bash
nuget restore
msbuild /p:Configuration=Release
```

或者使用 Visual Studio 打开项目进行生成亦可。

## 配置文件

在程序的工作目录中需要有 `config.json`，内容如下：

```json
{
	"ModelDirectory": "models",
	"LambdaRatio": "0.75",
	"CandidatesEachStep": "10"
}
```

`ModelDirectory` 指定程序（生成和加载）模型的目录位置（相对工作目录），`LambdaRatio` 指定混合的 n-gram 算法中进行非线性混合时使用的系数（详见 [实验报告](../README.md) ），`CandidateEachStep` 指定每扩展一个字后留下的可能选项数。

## 功能说明

直接运行编译出的 `NaiveIME.exe` 即可看到功能说明，共有 `interative`, `model`, `statistics`, `solve`, `analyze`, `merge`, `build`, `test`  八个实际的功能，每个功能的说明均已经给出。具体参数可以使用类似 `NaiveIME.exe solve --help` 的方法进行查询 。

本程序的训练数据格式为 UTF-8 编码的纯文本，每个句子分为两行，一行为空格分隔的拼音，一行为文本。常规的步骤为：

1. 生成词频统计

```bash
NaiveIME.exe analyze text/FILE_1.txt text/FILE_2.txt -d stats/
NaiveIME.exe merge stats/FILE_1_stats.csv stats/FILE_2_stats.csv -o stats/stats.csv
# 如果每个文本的数据量不大，上面两步也可以简化为
NaiveIME.exe analyze text/FILE_1.txt text/FILE_2.txt --merge --out stats/stats.csv
```

需要注意的是，merge 操作需要占用较大的内存，在 Windows 下可能受到单对象不得超过 2GB 的限制而失败，此时可以使用 Linux 进行分析。将分析与合并分为两部的原因是，本程序是单线程的，可以使用 `GNU Parallel` 等工具帮助进行并行分析后统一合并。

2. 生成模型

```bash
NaiveIME.exe build stats/stats.csv -m 1 2 3
```

模型会保存在上述的配置文件指定的目录下。

3. 进行拼音转换

```bash
# 交互式转换
NaiveIME.exe interactive -m MODEL
# 转换文件
NaiveIME.exe solve --in data/input.txt --out data/output.txt -m MODEL
```

其中模型可从 `1/2/12l/12m/123l` 中挑选，意义可见实验报告中。

4. 进行模型测试

```bash
NaiveIME.exe test -m MODEL_1 MODEL_2 --in data/test.txt --out data/result.txt
```

测试集格式与训练集相同，可以指定多个模型，如果不指定 `--out` ，则结果会输出到控制台。对于一组测试，输出如下：

```
------- IME Test Report -------
No.0: [NGram1][]
No.1: [NGram2][]
No.2: [NGramMixed][12m]
No.3: [NGramMixed][12l]
No.4: [NGramMixed][123l]
-------------------------------
> 在去年驻韩美军在韩国部署萨德系统一度引发多方面的争议
0 ＋区＋主汉＋＋＋汉＋不数＋的＋通＋＋因＋＋＋＋＋政一
1 ＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋
2 ＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋
3 ＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋
4 ＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋＋

...... （更多测试结果）

Results:
0: [NGram1][]:	Char accuracy: 0.5110656 Sentence accuracy: 0
1: [NGram2][]:	Char accuracy: 0.8389344 Sentence accuracy: 0.1147541
2: [NGramMixed][12m]:	Char accuracy: 0.8467213 Sentence accuracy: 0.1065574
3: [NGramMixed][12l]:	Char accuracy: 0.8456967 Sentence accuracy: 0.1065574
4: [NGramMixed][123l]:	Char accuracy: 0.9122951 Sentence accuracy: 0.1721312
```

内容几乎都是自解释的，“+”代表模型给出了正确的结果，最后有各个模型的字/句正确率分析。

5. 额外功能

   此外，可以使用 `model` 查询在给定前缀下各个输出的概率（输入格式为类似于 `qinghuada 学`）

   可以用 `statistic` 读取统计文件获取某一个短语的频率。 

## 其他

当输入法无法进行转换时，会输出当前的结果，后面以“？”填充满拼音音节长度，如：

```
wo jue de Android bu hao
我觉得？？？
```

请注意，一旦出现错误，后面的拼音就不会再被转换。