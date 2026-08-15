import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { IconComponent, IconName } from '../../shared/components/icon.component';

interface NavItem {
  label: string;
  path: string;
  icon: IconName;
  comingSoon?: boolean;
}

@Component({
  selector: 'hero-sidebar',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, RouterLinkActive, IconComponent],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
})
export class SidebarComponent {
  // Items flagged comingSoon are rendered disabled - they land in Phases 7-10.
  protected readonly navItems: NavItem[] = [
    { label: 'All Heroes', path: '/superheroes', icon: 'grid' },
    { label: 'Dashboard', path: '/dashboard', icon: 'chart', comingSoon: true },
    { label: 'Compare', path: '/battles', icon: 'swords', comingSoon: true },
    { label: 'Teams', path: '/teams', icon: 'users', comingSoon: true },
    { label: 'Missions', path: '/missions', icon: 'target', comingSoon: true },
    { label: 'Rankings', path: '/rankings', icon: 'trophy', comingSoon: true },
  ];
}
