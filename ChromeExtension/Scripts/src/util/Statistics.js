var Statistics = (function () {
    function Statistics() {
    }
    Statistics.getNumWithSetDec = function (num, numOfDec) {
        var pow10s = Math.pow(10, numOfDec || 0);
        return (numOfDec) ? Math.round(pow10s * num) / pow10s : num;
    };
    Statistics.getAverageFromNumArr = function (numArr, numOfDec) {
        var i = numArr.length, sum = 0;
        while (i--) {
            sum += numArr[i];
        }
        return Statistics.getNumWithSetDec((sum / numArr.length), numOfDec);
    };
    Statistics.getVariance = function (numArr, numOfDec) {
        var avg = Statistics.getAverageFromNumArr(numArr, numOfDec), i = numArr.length, v = 0;
        while (i--) {
            v += Math.pow((numArr[i] - avg), 2);
        }
        v /= numArr.length;
        return Statistics.getNumWithSetDec(v, numOfDec);
    };
    Statistics.isWithinStd = function (num, dist, std) {
        return num >= dist * std;
    };
    Statistics.getStandardDeviation = function (numArr, numOfDec) {
        var stdDev = Math.sqrt(Statistics.getVariance(numArr, numOfDec));
        return Statistics.getNumWithSetDec(stdDev, numOfDec);
    };
    return Statistics;
})();
