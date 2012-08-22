using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingTracer
{
    public static class Extensions
    {
        public static double Variance(this ICollection<double> values, bool unbiased = true)
        {
            if (values.Count == 0 || (unbiased && values.Count == 1))
            {
                return 0;
            }
            var ave = values.Average();
            double acc = 0;
            double div = unbiased ? values.Count - 1 : values.Count;
            foreach (var item in values)
            {
                var delta = item - ave;
                acc += delta * delta;
            }
            return acc / div;
        }

        public static double WeightedAverage(this IList<double> values, IList<double> weights)
        {
            if (values.Count != weights.Count) throw new ArgumentException("Two collections count must be equal each other");
            if (weights.Count == 0) throw new ArgumentException("An average for empty collection is not defined.");
            double valAcc = 0;
            double divAcc = 0;
            for (int i = 0; i < values.Count; i++)
            {
                valAcc += values[i] * weights[i];
            }
            for (int i = 0; i < weights.Count; i++)
            {
                divAcc += weights[i];
            }
            if (divAcc == 0)
            {
                divAcc = 1;
            }
            return valAcc / divAcc;
        }

        public static double WeightedVariance(this IList<double> values, IList<double> weights, bool unbiased = true)
        {
            if (values.Count != weights.Count) throw new ArgumentException("Two collections count must be equal each other");
            if (values.Count == 0 || (unbiased && values.Count == 1))
            {
                return 0;
            }
            var ave = values.Average();
            double acc = 0;
            double div = unbiased ? values.Count - 1 : values.Count;
            for (int i = 0; i < values.Count; i++)
            {
                var delta = values[i] - ave;
                acc += (delta * delta) * weights[i];
            }
            
            return acc / div;

        }

        public static double WeightedStandardDiviation(this IList<double> values, IList<double> weights)
        {
            return Math.Sqrt(WeightedVariance(values, weights));
        }
        
        public static double StandardDiviation(this ICollection<double> source)
        {
            return Math.Sqrt(source.Variance());
        }
    }

}
