import matplotlib.pyplot as plt
from matplotlib.ticker import FuncFormatter
import math

# Example data (replace with your actual data)
data = {
    'gossip': {
        'Line Topology': [(1000, 16312), (2000, 45314), (3000, 59578), (4000, 83197), (5000, 58718), (6000, 83716)],
        'Full Topology': [(1000, 1038), (2000, 2202), (3000, 2702), (4000, 3202), (5000, 3502), (6000, 4000)],
        '3D Topology': [(1000, 322), (2000, 369), (3000, 394),(4000, 523), (5000, 773), (6000, 508)],
        '2D Topology': [(1000, 583), (2000, 674), (3000, 1233), (4000, 1272), (5000, 1493), (6000, 2041)]
    },

    'pushsum': {
        'Line Topology': [(10, 35856), (20, 148659), (30, 328455), (40, 396106), (50, 863408), (60, 623697)],
        'Full Topology': [(10, 5248), (20, 11473), (30, 5252), (40, 41101), (50, 6243), (60, 8751)],
        '2D Topology': [(10, 17151), (20, 20505), (30, 31752), (40, 41517), (50, 56354), (60, 55852)],
        '3D Topology': [(10, 11103), (20, 14161), (30, 20950), (40, 27924), (50, 19751), (60, 21295)],
    },
}

def format_ticks(value, _):
    if value == 0:
        return '0'
    
    exponent = int(math.log10(value))
    return fr'$10^{{{exponent}}}$'

def plot_graph(data, algorithm):
    plt.figure(figsize=(10, 6))
    for topology in data[algorithm]:
        sizes, times = zip(*data[algorithm][topology])
        plt.plot(sizes, times, marker='o', label=topology)
    
    if algorithm.capitalize() == 'Gossip':
        plt.title(f'Gossip - Convergence Time in Different Topologies Including Full Network Topology')
    else:
        plt.title(f'Push Sum - Convergence Time in Different Topologies')
    plt.xlabel('Number of Nodes')
    plt.ylabel('Convergence Time (Milliseconds)') 
    plt.yscale('log')
    plt.gca().yaxis.set_major_formatter(FuncFormatter(format_ticks))
    plt.legend()
    plt.grid(True)
    plt.show()

# Plot for Gossip algorithm

plot_graph(data, 'gossip')

# Plot for Pushsum algorithm
plot_graph(data, 'pushsum')