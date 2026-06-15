using HIS.Models.Entities;

namespace HIS.Services.Helpers
{
    /// <summary>
    /// 遗传算法 - 医生排班优化
    /// 染色体：每个基因 = {医生ID, 星期几(0-6), 时段}
    /// 适应度：科室覆盖率 + 工作量均衡 + 不超限号
    /// </summary>
    public class ScheduleGA
    {
        // 数据源
        private readonly List<DoctorInfo> _doctors;      // 所有医生信息
        private readonly List<long> _deptIds;            // 需要排班的科室ID列表

        // 算法参数
        private readonly string[] _timeSlots = { "上午", "下午" };  // 每天的时段
        private readonly Random _rnd = new();                       // 随机数生成器
        private const int PopSize = 50;          // 种群大小（每代保留50个方案）
        private const int Generations = 200;     // 进化代数（迭代200次）
        private const double CrossoverRate = 0.8;  // 交叉率（80%概率交叉）
        private const double MutationRate = 0.15;  // 变异率（15%概率变异）

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="doctors">所有医生列表</param>
        /// <param name="deptIds">需要排班的科室ID列表</param>
        public ScheduleGA(List<DoctorInfo> doctors, List<long> deptIds)
        {
            _doctors = doctors;
            _deptIds = deptIds;
        }

        /// <summary>
        /// 运行遗传算法，返回最优排班方案
        /// </summary>
        public List<Schedule> Run()
        {
            // 1. 初始化种群：随机生成50个排班方案
            var population = InitializePopulation();

            // 记录当前最优方案
            Chromosome best = population[0];

            // 2. 进化循环（迭代200代）
            for (int gen = 0; gen < Generations; gen++)
            {
                // 计算每个方案的适应度分数
                foreach (var c in population)
                    c.Fitness = CalcFitness(c);

                // 按适应度从高到低排序
                population = population.OrderByDescending(c => c.Fitness).ToList();

                // 更新全局最优方案
                if (population[0].Fitness > best.Fitness)
                    best = population[0];

                // 如果最优方案已经达到0.95分以上，提前结束（不需要再进化）
                if (best.Fitness >= 0.95)
                    break;

                // 3. 生成新一代种群
                var newPop = new List<Chromosome>();

                // 精英保留：直接保留前两个最好的方案（防止好方案丢失）
                newPop.Add(population[0]);
                newPop.Add(population[1]);

                // 用选择、交叉、变异生成剩下的48个方案
                while (newPop.Count < PopSize)
                {
                    // 选择：从种群中选出两个"父母"方案
                    var p1 = TournamentSelect(population);
                    var p2 = TournamentSelect(population);

                    // 交叉：父母交换部分基因，生出一个孩子
                    var child = _rnd.NextDouble() < CrossoverRate
                        ? Crossover(p1, p2)   // 80%概率交叉
                        : p1.Clone();          // 20%概率直接复制父母

                    // 变异：随机修改孩子的一处基因
                    if (_rnd.NextDouble() < MutationRate)
                        Mutate(child);

                    newPop.Add(child);
                }

                // 新种群替换旧种群，进入下一代
                population = newPop;
            }

            // 4. 返回最优方案转换为Schedule列表
            return ChromosomeToSchedules(best);
        }

        /// <summary>
        /// 初始化种群：随机生成PopSize个排班方案
        /// </summary>
        private List<Chromosome> InitializePopulation()
        {
            var pop = new List<Chromosome>();
            for (int i = 0; i < PopSize; i++)
                pop.Add(new Chromosome(RandomGenes()));
            return pop;
        }

        /// <summary>
        /// 随机生成一组基因（一个完整的排班方案）
        /// 结构：每个科室 × 每周5个工作日 × 上下午时段 = 每个时段随机安排一个医生
        /// </summary>
        private List<(long DoctorId, int DayOfWeek, string TimeSlot)> RandomGenes()
        {
            var genes = new List<(long, int, string)>();

            // 遍历每个需要排班的科室
            foreach (var deptId in _deptIds)
            {
                // 获取该科室下所有在职医生
                var deptDocs = _doctors.Where(d => d.DepartmentId == deptId && d.Status == 1).ToList();
                if (!deptDocs.Any()) continue;  // 该科室没有医生，跳过

                // 周一到周五（DayOfWeek: 1~5）
                for (int d = 1; d <= 5; d++)
                {
                    // 上午和下午两个时段
                    foreach (var slot in _timeSlots)
                    {
                        // 随机从该科室的医生中选一个，作为这个时段的值班医生
                        genes.Add((deptDocs[_rnd.Next(deptDocs.Count)].Id, d, slot));
                    }
                }
            }
            return genes;
        }

        /// <summary>
        /// 计算适应度分数（0~1之间，越高越好）
        /// 评估一个排班方案的好坏
        /// </summary>
        private double CalcFitness(Chromosome c)
        {
            double score = 0;

            // 按【星期+时段】分组，检查每个时段各科室的覆盖情况
            var groups = c.Genes.GroupBy(g => new { g.DayOfWeek, g.TimeSlot });

            foreach (var g in groups)
            {
                // 计算这个时段覆盖了哪些科室
                var deptCovered = new HashSet<long>();
                foreach (var gene in g)
                {
                    var doc = _doctors.FirstOrDefault(d => d.Id == gene.DoctorId);
                    if (doc != null) deptCovered.Add(doc.DepartmentId);
                }
                // 科室覆盖率得分（占50%权重）
                score += (double)deptCovered.Count / _deptIds.Count * 0.5;

                // 检查医生工作量是否均衡（不超限号）
                foreach (var docGroup in g.GroupBy(x => x.DoctorId))
                {
                    var doc = _doctors.FirstOrDefault(d => d.Id == docGroup.Key);
                    if (doc != null && docGroup.Count() <= doc.MaxDailyPatients / 15)
                        score += 0.3 / g.Count();  // 工作量合理，加分
                }
            }

            // 惩罚项：同一个医生在同一时间被安排到两个不同地方（冲突）
            var duplicates = c.Genes
                .GroupBy(g => new { g.DoctorId, g.DayOfWeek, g.TimeSlot })
                .Count(g => g.Count() > 1);
            score -= duplicates * 0.1;

            // 确保分数在0~1之间
            return Math.Max(0, Math.Min(1, score));
        }

        /// <summary>
        /// 锦标赛选择：随机选3个方案，返回其中适应度最高的
        /// （模拟自然选择：强者生存）
        /// </summary>
        private Chromosome TournamentSelect(List<Chromosome> pop)
        {
            var candidates = Enumerable.Range(0, 3)
                .Select(_ => pop[_rnd.Next(pop.Count)])
                .ToList();
            return candidates.OrderByDescending(c => c.Fitness).First().Clone();
        }

        /// <summary>
        /// 交叉操作：父母交换部分基因，生出一个孩子
        /// 随机选一个切割点，孩子前半段来自父亲，后半段来自母亲
        /// </summary>
        private Chromosome Crossover(Chromosome p1, Chromosome p2)
        {
            int point = _rnd.Next(p1.Genes.Count);  // 随机切割点
            var child = new Chromosome(new List<(long, int, string)>());
            child.Genes.AddRange(p1.Genes.Take(point));   // 前半段来自父亲
            child.Genes.AddRange(p2.Genes.Skip(point));   // 后半段来自母亲
            return child;
        }

        /// <summary>
        /// 变异操作：随机修改一个基因
        /// 把某个时段的医生换成同科室的另一个医生
        /// </summary>
        private void Mutate(Chromosome c)
        {
            int idx = _rnd.Next(c.Genes.Count);  // 随机选一个基因
            var old = c.Genes[idx];

            // 找到这个医生所在的科室
            var deptId = _doctors.FirstOrDefault(x => x.Id == old.DoctorId)?.DepartmentId;
            if (deptId == null) return;

            // 获取同科室的其他医生
            var deptDocs = _doctors.Where(d => d.DepartmentId == deptId && d.Status == 1).ToList();
            if (deptDocs.Any())
            {
                // 换成同科室的另一个医生（可能换成自己，也可能换成别人）
                var newDoc = deptDocs[_rnd.Next(deptDocs.Count)];
                c.Genes[idx] = (newDoc.Id, old.DayOfWeek, old.TimeSlot);
            }
        }

        /// <summary>
        /// 将染色体（基因列表）转换成Schedule实体列表
        /// 用于保存到数据库或返回给前端
        /// </summary>
        private List<Schedule> ChromosomeToSchedules(Chromosome c)
        {
            return c.Genes.Select(g =>
            {
                var doc = _doctors.First(d => d.Id == g.DoctorId);
                return new Schedule
                {
                    DoctorId = g.DoctorId,
                    DepartmentId = doc.DepartmentId,
                    DayOfWeek = g.DayOfWeek,
                    TimeSlot = g.TimeSlot,
                    MaxPatients = 30,   // 默认限号30人
                    Status = 1
                };
            }).ToList();
        }

        /// <summary>
        /// 染色体类（一个完整的排班方案）
        /// </summary>
        private class Chromosome
        {
            public List<(long DoctorId, int DayOfWeek, string TimeSlot)> Genes;  // 基因序列
            public double Fitness;  // 适应度分数

            public Chromosome(List<(long, int, string)> genes)
            {
                Genes = genes;
            }

            /// <summary>深度复制</summary>
            public Chromosome Clone()
            {
                return new Chromosome(Genes.ToList());
            }
        }
    }
}