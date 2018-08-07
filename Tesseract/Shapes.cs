namespace Tesseract {
    public class Shapes {
        public static Matrix[] Cube() {
            return new Matrix[] {
                new Matrix(new double[] { -1, -1, -1}), // Back face
                new Matrix(new double[] { -1,  1, -1}),
                new Matrix(new double[] {  1,  1, -1}),
                new Matrix(new double[] {  1, -1, -1}),

                new Matrix(new double[] { -1, -1,  1}), // Front face
                new Matrix(new double[] { -1,  1,  1}),
                new Matrix(new double[] {  1,  1,  1}),
                new Matrix(new double[] {  1, -1,  1}),
            };
        }

        public static Matrix[] Hypercube() {
            return new Matrix[] {
                new Matrix(new double[] { -1, -1, -1,  1}), // Back face
                new Matrix(new double[] { -1,  1, -1,  1}),
                new Matrix(new double[] {  1,  1, -1,  1}),
                new Matrix(new double[] {  1, -1, -1,  1}),

                new Matrix(new double[] { -1, -1,  1,  1}), // Front face
                new Matrix(new double[] { -1,  1,  1,  1}),
                new Matrix(new double[] {  1,  1,  1,  1}),
                new Matrix(new double[] {  1, -1,  1,  1}),

                new Matrix(new double[] { -1, -1, -1, -1}), // 4th dimension, face #1
                new Matrix(new double[] { -1,  1, -1, -1}),
                new Matrix(new double[] {  1,  1, -1, -1}),
                new Matrix(new double[] {  1, -1, -1, -1}),

                new Matrix(new double[] { -1, -1,  1, -1}), // 4th dimension, face #2
                new Matrix(new double[] { -1,  1,  1, -1}),
                new Matrix(new double[] {  1,  1,  1, -1}),
                new Matrix(new double[] {  1, -1,  1, -1})
            };
        }
    }
}
