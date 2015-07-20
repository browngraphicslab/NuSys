class Statistics {
    
    static getNumWithSetDec(num:number, numOfDec:number):number {
        var pow10s = Math.pow(10, numOfDec || 0);
        return (numOfDec) ? Math.round(pow10s * num) / pow10s : num;
    }

    static getAverageFromNumArr(numArr:Array<number>, numOfDec:number):number {
        var i = numArr.length,
            sum = 0;
        while (i--) {
            sum += numArr[i];
        }
        return Statistics.getNumWithSetDec((sum / numArr.length), numOfDec);
    }

    static getVariance(numArr:Array<number>, numOfDec:number):number {
        var avg = Statistics.getAverageFromNumArr(numArr, numOfDec),
            i = numArr.length,
            v = 0;

        while (i--) {
            v += Math.pow((numArr[i] - avg), 2);
        }
        v /= numArr.length;
        return Statistics.getNumWithSetDec(v, numOfDec);
    }

    static isWithinStd(num:number, dist:number, std:number) {
        return num >= dist * std;
    }

    static getStandardDeviation(numArr:Array<number>, numOfDec:number):number {
        var stdDev = Math.sqrt(Statistics.getVariance(numArr, numOfDec));
        return Statistics.getNumWithSetDec(stdDev, numOfDec);
    }
}