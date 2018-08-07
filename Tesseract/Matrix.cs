using System;

namespace Tesseract {
    public class Matrix {
        public readonly int Rows;
        public readonly int Columns;
        public readonly double[][] Data;

        public Matrix(int rows, int columns) {
            Rows = rows;
            Columns = columns;
            Data = new double[rows][];
            for(int m = 0; m < rows; m++) Data[m] = new double[columns];
        }

        public Matrix(double[][] data) : this(data.Length, data[0].Length) {
            for(int m = 0; m < Rows; m++) {
                for(int n = 0; n < Columns; n++) {
                    Data[m][n] = data[m][n];
                }
            }
        }

        public Matrix(double[] data) : this(data.Length, 1) { //  Column Matrix
            for(int m = 0; m < Rows; m++) {
                Data[m][0] = data[m];
            }
        }

        public double Determinant() {
            if(Rows == 2 && Columns == 2) return Data[0][0] * Data[1][1] - Data[0][1] * Data[1][0];

            double d = 0;
            for(int n = 0; n < Columns; n++) {
                Matrix mM = Minor(0, n);
                d += Data[0][n] * mM.Determinant() * (n % 2 == 0 ? 1 : -1);
            }

            return d;
        }

        public Matrix Minor(int row, int column) {
            Matrix mM = new Matrix(Rows - 1, Columns - 1);
            int m2 = 0;
            for(int m1 = 0; m1 < Rows; m1++) {
                if(m1 != row) {
                    int n2 = 0;
                    for(int n1 = 0; n1 < Columns; n1++) {
                        if(n1 != column) {
                            mM.Data[m2][n2] = Data[m1][n1];
                            n2++;
                        }
                    }
                    m2++;
                }
            }
            return mM;
        }

        public Matrix Inverse() {
            Matrix mM = new Matrix(Rows, Columns);
            double d = Determinant();

            if(Rows==2) {
                return new Matrix(
                    new double[][] { new double[] {  Data[1][1], -Data[0][1] },
                                     new double[] { -Data[1][0],  Data[0][0] }
                    });
            }

            for(int m = 0; m < Rows; m++) { // Transposed Matrix of Minors and Cofactors
                for(int n = 0; n < Columns; n++) {
                    mM.Data[n][m] = Minor(m, n).Determinant();
                    if(m % 2 == 0) {
                        if(n % 2 != 0) mM.Data[n][m] *= -1;
                    } else {
                        if(n % 2 == 0) mM.Data[n][m] *= -1;
                    }
                }
            }

            return mM / d;
        }

        private Matrix R(int row, int col, double angle) {
            Matrix mR = new Matrix(Rows, Columns);

            for(int m = 0; m < mR.Rows; m++) {
                for(int n = 0; n < mR.Columns; n++) {
                    if(m == row && n == row) {
                        mR.Data[m][n] = Math.Cos(angle);
                    } else if(m == col && n == col) {
                        mR.Data[m][n] = Math.Cos(angle);
                    } else if(m == row && n == col) {
                        mR.Data[m][n] = -Math.Sin(angle);
                    } else if(m == col && n == row) {
                        mR.Data[m][n] = Math.Sin(angle);
                    } else if(m == n && m != row && n != col) {
                        mR.Data[m][n] = 1;
                    }
                }
            }

            return mR;
        }

        // Aguilera-Pérez Algorithm
        // http://wscg.zcu.cz/wscg2004/Papers_2004_Short/N29.pdf
        public Matrix Rotate(double angle, int n) {
            if(n <= 2) {
                if(n == 0) {
                    return this * R(n, 2, angle) * this.Inverse();
                } else {
                    return this * R(n - 1, n, angle) * this.Inverse();
                }

            }

            Matrix[] mM = new Matrix[Rows * Columns];
            Matrix[] v = new Matrix[Rows * Columns];

            v[0] = this;
            mM[1] = Inverse();
            v[1] = this * mM[1];
            mM[0] = mM[1];

            int k = 1;
            for(int r = 2; r < n; r++) {
                for(int c = n; c >= r; c--) {
                    k++;

                    mM[k] = R(c, c - 1, Math.Atan2(v[k - 1].Data[r][c], v[k - 1].Data[r][c - 1]));
                    v[k] = v[k - 1] * mM[k];
                    mM[0] *= mM[k];
                }
            }
            return mM[0] * R(n - 1, n, angle) * mM[0].Inverse();
        }

        public static Matrix Identity(int rows, int columns) {
            Matrix r = new Matrix(rows, columns);
            for(int m = 0, n = 0; m < rows && n < columns; m++, n++) {
                r.Data[m][n] = 1;
            }
            return r;
        }

        public static Matrix operator *(Matrix mA, double s) {
            Matrix mR = new Matrix(mA.Data);
            for(int m = 0; m < mR.Rows; m++) {
                for(int n = 0; n < mR.Columns; n++) {
                    mR.Data[m][n] = (double)mR.Data[m][n] * (double)s;
                }
            }
            return mR;
        }

        public static Matrix operator *(Matrix mA, Matrix mB) { //  Dot Product
            Matrix mR;

            if(mA.Columns == mB.Rows) {
                mR = new Matrix(mA.Rows, mB.Columns);
            } else if(mA.Rows == mB.Columns) {
                return mB * mA;
            } else {
                throw new ArgumentException("The number of rows in m1 must match the number rows of m2");
            }

            for(int m = 0; m < mR.Rows; m++) {
                for(int n = 0; n < mR.Columns; n++) {

                    for(int m1 = 0; m1 < mB.Rows; m1++) {
                        mR.Data[m][n] += mA.Data[m][m1] * mB.Data[m1][n];
                    }

                }
            }
            return mR;
        }

        public static Matrix operator /(Matrix m1, double s) {
            return m1 * (1 / s);
        }

        public string Print() {
            string result = "";
            int max = 0;
            for(int m = 0; m < Rows; m++) {
                for(int n = 0; n < Columns; n++) {
                    max = Math.Max(max, Data[m][n].ToString().Length);
                }
            }

            result = $"{Rows}x{Columns}" + Environment.NewLine;
            result += "┌" + new string(' ', (max + 1) * Columns) + "┐" + Environment.NewLine;
            for(int m = 0; m < Rows; m++) {
                result += "│";
                for(int n = 0; n < Columns; n++) {
                    result += Data[m][n].ToString().PadLeft(max) + " ";
                }
                result += "│" + Environment.NewLine;
            }
            result += "└" + new string(' ', (max + 1) * Columns) + "┘" + Environment.NewLine;
            return result;
        }

        public override string ToString() {
            return $"{Rows}x{Columns}";
        }
    }
}
