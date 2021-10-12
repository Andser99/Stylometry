import matplotlib.pyplot as plt
import seaborn as sns
import pandas as pd
import numpy as np
import scipy.stats as stats
import warnings
warnings.filterwarnings('ignore')
df = pd.read_csv('data/data.csv')
plt.rcParams["figure.figsize"] = (15,15)
# ax = df.plot(kind='scatter', x=yName, y=xName)
# plt.show()
connectDots = False
colors='bckgmrybckgmrybckgmrybckgmrybckgmrybckgmrybckgmrybckgmrybckgmrybckgmry'
def connectpoints(x,y,p1,p2):
    x1, x2 = x[p1], x[p2]
    y1, y2 = y[p1], y[p2]
    plt.plot([x1,x2],[y1,y2],'k-')

for x in range(1, len(df.columns)):
    print('Y = misspellratio')
    xName = df.columns[x]
    yName = 'misspellratio'

    ax = df.set_index(xName)[yName].plot(style='o', color='red', ms=3)

    a = pd.concat({xName: df[xName], yName: df[yName], 'Author': df['Author']}, axis=1)
    
    rows = []
    for i, point in a.iterrows():
        rows.append((i, point))
    for i, point in rows:
        if i % 2 == 0 and connectDots:
            plt.plot([point[xName],rows[i+1][1][xName]],[point[yName],rows[i+1][1][yName]], color=colors[int(i/2)])
        ax.text(point[xName], point[yName], str(int(point['Author'])), fontsize=12)

    f = plt.figure()
    f.set_figwidth(10)
    f.set_figheight(10)
    plt.show()