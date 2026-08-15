import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { IconComponent, IconName } from '../../shared/components/icon.component';

interface NavItem {
  label: string;
  path: string;
  icon: IconName;
}

@Component({
  selector: 'hero-sidebar',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, RouterLinkActive, IconComponent],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
})
export class SidebarComponent {
  protected readonly navItems: NavItem[] = [
    { label: 'Dashboard', path: '/dashboard', icon: 'chart' },
    { label: 'All Heroes', path: '/superheroes', icon: 'grid' },
    { label: 'Battle', path: '/battles', icon: 'swords' },
    { label: 'Missions', path: '/missions', icon: 'target' },
    { label: 'Rankings', path: '/rankings', icon: 'trophy' },
    { label: 'Powers', path: '/powers', icon: 'bolt' },
    { label: 'Teams', path: '/teams', icon: 'users' },
  ];
}
