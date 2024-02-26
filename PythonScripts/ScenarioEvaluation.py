import csv
import matplotlib.pyplot as plt
import numpy as np

# path to file that sould evaluate
PATH_TO_CSV_FILE = 'csvFiles/240108150002scenarioResult.csv'
PATH_TO_CSV_FILE = 'csvFiles/240128141848scenarioResult.csv'
PLOT_TITLE = 'Erfolgsquote der einzelnen Szenarien: Agent 05'

REG_GOAL = 0
REG_TIME = 1
REG_FAIL = 2

num_scenarios = 13
scenario = [[0, 0, 0] for _ in range(num_scenarios)]

with open(PATH_TO_CSV_FILE) as csv_file:
    # read csv file
    csv_reader = csv.reader(csv_file, delimiter=';')
    
    # skip headline
    next(csv_reader)

    # count timerout, goals and fails for each scenario
    num_rows = 0
    for row in csv_reader:
        num_rows += 1
        for i, column in enumerate(row):
            if column == 'Goal':
                scenario[i][REG_GOAL] += 1
            elif column == 'timeout':
                scenario[i][REG_TIME] += 1
            else:
                scenario[i][REG_FAIL] += 1

scen_goal_perc = []
scen_fails_perc = []

for i in range(num_scenarios):
    total = scenario[i][REG_GOAL] + scenario[i][REG_TIME] + scenario[i][REG_FAIL]
    goal_percent = round(scenario[i][REG_GOAL] / total * 100, 1)
    rest_percent = round(100 - goal_percent, 1)
    scen_goal_perc.append(goal_percent)
    scen_fails_perc.append(rest_percent)

# Balkendiagramm erstellen
bar_width = 0.35
index = np.arange(num_scenarios)
fig, ax = plt.subplots()

bar1 = ax.bar(index, scen_goal_perc, bar_width, label='Treffer', color='green')  # Grün für Treffer
bar2 = ax.bar(index + bar_width, scen_fails_perc, bar_width, label='Fehlschläge', color='#d68100')  # Orange für Fehlschläge

#ax.set_xlabel('Szenarien')
ax.set_ylabel('Trefferquote in %')
ax.set_title(PLOT_TITLE)
ax.set_xticks(index + bar_width / 2)
ax.set_xticklabels([f'Szenario {i}' for i in range(num_scenarios)], rotation=45, ha="right")
ax.set_ylim(0, 119)
ax.legend(loc='upper left', bbox_to_anchor=(0, 1))

# Anzeige der Balkenwerte
for rect in bar1 + bar2:
    height = rect.get_height()
    ax.annotate('{}'.format(height),
                xy=(rect.get_x() + rect.get_width() / 2, height),
                xytext=(0, 3),  # 3 Punkte vertikal nach oben verschieben
                textcoords="offset points",
                ha='center', va='bottom')

def on_close(event):
    if event.key == 'escape':
        plt.close()

fig.canvas.mpl_connect('key_press_event', on_close)

# set bottom padding to 0.15
plt.subplots_adjust(bottom=0.17)
plt.show()
