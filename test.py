
import numpy as np

import matplotlib.pyplot as plt

test = np.loadtxt("testim500.txt")
print(test)

plt.imshow(test.reshape(28,28))
